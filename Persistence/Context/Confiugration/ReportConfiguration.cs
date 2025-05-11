using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Confiugration
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.Property(r => r.Type)
                .IsRequired()
                .HasConversion<string>(); // Store enum as string

            builder.Property(r => r.GeneratedDate)
                .IsRequired();

            builder.Property(r => r.Content)
                .IsRequired()
                .HasMaxLength(1000);

            // Relationships
            builder.HasOne(r => r.Doctor)
                .WithMany()
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
