

using EmployeeManagementSys.DL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeManagementSys.BL.Service
{
    public class JWTService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<Employee> _userManager;

        public JWTService(IConfiguration configuration, UserManager<Employee> userManager)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<string> GenerateTokenAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // Get user roles
            var roles = await _userManager.GetRolesAsync(employee);
            var userRole = roles.FirstOrDefault() ?? "Employee";

            // Create claims
            var claims = new List<Claim>
            {
               
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Email, employee.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, employee.UserName ?? string.Empty),
                new Claim(ClaimTypes.GivenName, employee.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, employee.LastName ?? string.Empty),       
                new Claim(ClaimTypes.Role, userRole),
                new Claim("RequiresPasswordReset", employee.RequiresPasswordReset.ToString().ToLower()),
                new Claim("NationalId", employee.NationalId ?? string.Empty),
                new Claim("Age", employee.Age.ToString()),
                new Claim("Status", employee.Status.ToString()),
                
            };

            // Add additional role claims if user has multiple roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:IssuerSigningKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(GetTokenExpiryHours()),
                SigningCredentials = credentials,
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<string> RefreshTokenAsync(string currentToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validate the current token (without checking expiry)
                var validationParameters = GetTokenValidationParameters();
                validationParameters.ValidateLifetime = false; // Don't validate expiry for refresh

                var principal = tokenHandler.ValidateToken(currentToken, validationParameters, out SecurityToken validatedToken);

                // Get user ID from token
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    throw new SecurityTokenException("Invalid token: User ID not found");
                }

                // Get user from database
                var employee = await _userManager.FindByIdAsync(userId.ToString());
                if (employee == null || employee.Status != EmployeeStatus.Active)
                {
                    throw new SecurityTokenException("Invalid token: User not found or inactive");
                }

                // Generate new token
                return await GenerateTokenAsync(employee);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token for refresh", ex);
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters();

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true; // If we can't read the token, consider it expired
            }
        }

        public async Task<bool> IsTokenValidForUserAsync(string token, Guid userId)
        {
            var principal = ValidateToken(token);
            if (principal == null) return false;

            var tokenUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(tokenUserId, out Guid parsedUserId))
                return false;

            if (parsedUserId != userId) return false;

            // Additional check: ensure user still exists and is active
            var employee = await _userManager.FindByIdAsync(userId.ToString());
            return employee != null && employee.Status == EmployeeStatus.Active;
        }

        public Dictionary<string, string> GetTokenClaims(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                return jsonToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public TimeSpan GetTokenRemainingTime(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                var remaining = jsonToken.ValidTo - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JWT:IssuerSigningKey"])
                ),
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No tolerance for expiry
            };
        }

        private int GetTokenExpiryHours()
        {
            // Try to get from configuration, default to 24 hours
            if (int.TryParse(_configuration["JWT:ExpiryHours"], out int hours))
            {
                return hours;
            }
            return 24; // 
        }
    }
}

