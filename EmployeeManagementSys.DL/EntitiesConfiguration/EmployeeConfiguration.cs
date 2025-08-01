using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;



namespace EmployeeManagementSys.DL
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.NationalId)
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(e => e.Age)
                .IsRequired();


            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedDate)
                .IsRequired(false);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(EmployeeStatus.Active)
                .HasConversion<int>();

            // Ignore computed properties
            builder.Ignore(e => e.FullName);
            builder.Ignore(e => e.IsActive);
            builder.Ignore(e => e.StatusDisplayName);

            // Relationships
            builder.HasMany(e => e.AttendanceRecords)
                .WithOne(a => a.Employee)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NationalId)
                .IsUnique();
        }
    }
}
