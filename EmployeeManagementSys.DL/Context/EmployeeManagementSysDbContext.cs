using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EmployeeManagementSys.DL
{
    public class EmployeeManagementSysDbContext : IdentityDbContext<Employee, IdentityRole<Guid>, Guid>
    {
        public EmployeeManagementSysDbContext(DbContextOptions<EmployeeManagementSysDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeManagementSysDbContext).Assembly);
        }

        // tables
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Employee> Employees { get; set; }
    }
  
}
