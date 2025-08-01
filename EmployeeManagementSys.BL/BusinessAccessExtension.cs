using EmployeeManagementSys.BL.Service;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagementSys.BL
{
    public static class  BusinessAccessExtension
    {
        public static void AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeManager, EmployeeManager>();
            services.AddScoped<IAttendanceManager, AttendanceManager>();
            services.AddScoped<ISignatureManager, SignatureManager>();
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();


            services.AddScoped<JWTService>();

            services.AddValidatorsFromAssembly(typeof(BusinessAccessExtension).Assembly);

        }


    }
}
