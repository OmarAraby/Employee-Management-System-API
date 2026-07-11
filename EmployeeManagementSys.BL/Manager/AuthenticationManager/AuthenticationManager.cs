using System.Net;
using EmployeeManagementSys.BL.Service;
using EmployeeManagementSys.DL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementSys.BL
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly UserManager<Employee> _userManager;
        private readonly SignInManager<Employee> _signInManager;
        private readonly JWTService _jwtService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthenticationManager(
            UserManager<Employee> userManager,
            SignInManager<Employee> signInManager,
            JWTService jwtService,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<APIResult<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return new APIResult<LoginResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "InvalidCredentials", Message = "Email and password are required." } }
                };
            }

            var employee = await _userManager.FindByEmailAsync(loginDto.Email);
            if (employee == null || employee.Status != EmployeeStatus.Active)
            {
                return new APIResult<LoginResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "InvalidCredentials", Message = "Invalid email or password." } }
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(employee, loginDto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                string errorMessage = result.IsLockedOut
                    ? "Account is locked due to multiple failed attempts."
                    : "Invalid email or password.";

                return new APIResult<LoginResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "InvalidCredentials", Message = errorMessage } }
                };
            }

            // Generate JWT token
            var token = await _jwtService.GenerateTokenAsync(employee);
            var tokenExpiry = DateTime.UtcNow.AddHours(24); // Match your JWT configuration

            // Get user roles
            var roles = await _userManager.GetRolesAsync(employee);
            var userRole = roles.FirstOrDefault() ?? "Employee";

            return new APIResult<LoginResponseDto>
            {
                Success = true,
                Data = new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    EmployeeId = employee.Id,
                    FullName = employee.FullName,
                    Email = employee.Email,
                    Role = userRole,
                    RequiresPasswordReset = employee.RequiresPasswordReset,
                    Token = token,
                    TokenExpiry = tokenExpiry
                }
            };
        }

        public async Task<APIResult<ResetPasswordResponseDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                return new APIResult<ResetPasswordResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "PasswordMismatch", Message = "New password and confirmation do not match." } }
                };
            }

            var employee = await _userManager.FindByIdAsync(resetPasswordDto.EmployeeId.ToString());
            if (employee == null)
            {
                return new APIResult<ResetPasswordResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "NotFound", Message = "Employee not found." } }
                };
            }

            // Verify current password
            if (!await _userManager.CheckPasswordAsync(employee, resetPasswordDto.CurrentPassword))
            {
                return new APIResult<ResetPasswordResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "InvalidPassword", Message = "Current password is incorrect." } }
                };
            }

            // Reset password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(employee);
            var result = await _userManager.ResetPasswordAsync(employee, resetToken, resetPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new APIError { Code = "PasswordResetFailed", Message = e.Description }).ToArray();
                return new APIResult<ResetPasswordResponseDto>
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Update password reset flag and save changes
            employee.RequiresPasswordReset = false;
            employee.LastPasswordResetDate = DateTime.UtcNow;
            employee.UpdatedDate = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(employee);
            if (!updateResult.Succeeded)
            {
                return new APIResult<ResetPasswordResponseDto>
                {
                    Success = false,
                    Errors = updateResult.Errors.Select(e => new APIError { Code = "UpdateFailed", Message = e.Description }).ToArray()
                };
            }

            return new APIResult<ResetPasswordResponseDto>
            {
                Success = true,
                Data = new ResetPasswordResponseDto
                {
                    Success = true,
                    Message = "Password reset successfully."
                }
            };
        }

        public async Task<APIResult<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var newToken = await _jwtService.RefreshTokenAsync(refreshTokenDto.Token);
                var tokenExpiry = DateTime.UtcNow.AddHours(24);

                return new APIResult<RefreshTokenResponseDto>
                {
                    Success = true,
                    Data = new RefreshTokenResponseDto
                    {
                        Success = true,
                        Message = "Token refreshed successfully",
                        NewToken = newToken,
                        TokenExpiry = tokenExpiry
                    }
                };
            }
            catch (SecurityTokenException ex)
            {
                return new APIResult<RefreshTokenResponseDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "InvalidToken", Message = ex.Message } }
                };
            }
        }

        public async Task<APIResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            // Enumeration-safe: always report success regardless of whether the
            // email matches an account, so callers can't probe for valid emails.
            var success = new APIResult<bool>
            {
                Success = true,
                Data = true
            };

            if (string.IsNullOrWhiteSpace(forgotPasswordDto.Email))
            {
                return success;
            }

            var employee = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (employee == null || employee.Status != EmployeeStatus.Active)
            {
                return success;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(employee);
            var baseUrl = _configuration["App:ResetPasswordUrl"] ?? "http://localhost:8085/reset-password";
            var link = $"{baseUrl}?email={Uri.EscapeDataString(employee.Email!)}&token={Uri.EscapeDataString(token)}";

            var body =
                $"<p>Hello {WebUtility.HtmlEncode(employee.FirstName)},</p>" +
                "<p>We received a request to reset your Employee Management System password. " +
                $"Click the link below to choose a new password (valid for a limited time):</p>" +
                $"<p><a href=\"{link}\">Reset your password</a></p>" +
                "<p>If you didn't request this, you can safely ignore this email.</p>";

            await _emailSender.SendEmailAsync(employee.Email!, "Reset your password", body);
            return success;
        }

        public async Task<APIResult<bool>> ResetPasswordWithTokenAsync(ResetPasswordWithTokenDto resetDto)
        {
            if (string.IsNullOrWhiteSpace(resetDto.Email)
                || string.IsNullOrWhiteSpace(resetDto.Token)
                || string.IsNullOrWhiteSpace(resetDto.NewPassword))
            {
                return Fail("InvalidRequest", "Email, token and new password are required.");
            }
            if (resetDto.NewPassword != resetDto.ConfirmPassword)
            {
                return Fail("PasswordMismatch", "New password and confirmation do not match.");
            }

            var employee = await _userManager.FindByEmailAsync(resetDto.Email);
            if (employee == null)
            {
                // Generic message — do not reveal whether the account exists.
                return Fail("InvalidToken", "The reset link is invalid or has expired.");
            }

            var result = await _userManager.ResetPasswordAsync(employee, resetDto.Token, resetDto.NewPassword);
            if (!result.Succeeded)
            {
                var identityError = result.Errors.FirstOrDefault();
                // Identity's InvalidToken means bad/expired link; other codes are
                // password-policy failures worth surfacing.
                var (code, message) = identityError?.Code == "InvalidToken"
                    ? ("InvalidToken", "The reset link is invalid or has expired.")
                    : ("ResetFailed", identityError?.Description ?? "Could not reset the password.");
                return Fail(code, message);
            }

            employee.RequiresPasswordReset = false;
            employee.LastPasswordResetDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(employee);

            return new APIResult<bool> { Success = true, Data = true };
        }

        private static APIResult<bool> Fail(string code, string message) => new APIResult<bool>
        {
            Success = false,
            Errors = new[] { new APIError { Code = code, Message = message } }
        };
    }
}