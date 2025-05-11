using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Persistence.Context.Confiugration
{
    public class BillingConfigurations : IEntityTypeConfiguration<Billing>
    {
        public void Configure(EntityTypeBuilder<Billing> builder)
        {

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Status)
                .HasConversion<string>(); // Store enum as string

            // Relationships
            builder.HasMany(b => b.Payments)
                .WithOne(p => p.Billing)
                .HasForeignKey(p => p.BillingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Patient)
                .WithOne()
                .HasForeignKey<Billing>(b => b.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
