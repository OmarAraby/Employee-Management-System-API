using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementSys.DL
{
    public static class DataAccessExtension
    {
        public static void AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("db");

            // Register DbContext with connection string from configuration
            services.AddDbContext<EmployeeManagementSysDbContext>(options => options
                .UseSqlServer(connectionString));

            //Register repositories and other services
            
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            #region User Services
            services.AddIdentity<Employee, IdentityRole<Guid>>(options =>
            {
                #region Password Settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                #endregion
                #region Email Settings
                options.User.RequireUniqueEmail = true;
                #endregion
            }).AddEntityFrameworkStores<EmployeeManagementSysDbContext>();
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme =
                option.DefaultForbidScheme =
                option.DefaultScheme =
                option.DefaultChallengeScheme =
                option.DefaultSignInScheme =
                JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    #region Issuer
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration["JWT:IssuerSigningKey"])
                        ),
                    #endregion
                    #region Audience
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:Audience"], // change when create Angular
                    #endregion


                };
            });
            #endregion
        }




    }
}
