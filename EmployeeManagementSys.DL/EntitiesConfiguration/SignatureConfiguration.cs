

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementSys.DL
{
    public class SignatureConfiguration : IEntityTypeConfiguration<Signature>
    {
        public void Configure(EntityTypeBuilder<Signature> builder)

        {
            builder.ToTable("Signatures");
            builder.HasKey(s => s.SignatureId);
            builder.Property(a => a.FileName)
               .IsRequired()
               .HasMaxLength(255);

            builder.Property(a => a.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.UploadedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(s => s.Employee)
                .WithOne(e => e.Signature) 
                .HasForeignKey<Signature>(s => s.EmployeeId) // Specify the foreign key type explicitly
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
