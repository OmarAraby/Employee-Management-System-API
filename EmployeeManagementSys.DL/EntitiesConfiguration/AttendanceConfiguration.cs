using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementSys.DL.EntitiesConfiguration
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            //throw new NotImplementedException();

            builder.HasKey(a => a.AttendanceId);
            builder.Property(a => a.EmployeeId)
                .IsRequired();

            builder.Property(a => a.CheckInDate)
               .IsRequired()
               .HasColumnType("date"); // Store only date part

            builder.Property(a => a.CheckInTime)
                .IsRequired()
                .HasColumnType("time"); // Store only time part

            builder.Property(a => a.CheckOutTime)
                .IsRequired(false)
                .HasColumnType("time");

            builder.Property(a => a.WorkingHours)
                .IsRequired(false)
                .HasColumnType("decimal(4,2)");

            builder.Property(a => a.CreatedDate)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");


            // Ignore computed properties 
            builder.Ignore(a => a.CheckInDateString);
            builder.Ignore(a => a.CheckInTimeString);
            builder.Ignore(a => a.IsOnTime);
            builder.Ignore(a => a.IsLate);
            builder.Ignore(a => a.Status);


            //// Relationships
            //builder.HasOne(a => a.Employee)
            //    .WithMany(e => e.AttendanceRecords)
            //    .HasForeignKey(a => a.EmployeeId)
            //    .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => new { a.EmployeeId, a.CheckInDate }).IsUnique();
        }
    }
}
