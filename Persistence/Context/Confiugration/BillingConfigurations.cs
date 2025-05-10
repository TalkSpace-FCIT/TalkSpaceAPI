using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Enums; 

namespace Persistence.Context.Confiugration
{
    public class BillingConfigurations:IEntityTypeConfiguration<Billing>
    {
        public void Configure(EntityTypeBuilder<Billing> builder)
        {
            builder.Property(b => b.ServicesRendered)
          .HasConversion(
              v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
              v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
          );

            builder.Property(b => b.TotalCost)
                .HasPrecision(18,2);

            builder.Property(b => b.Status)
                .HasConversion<string>(); // Store enum as string

            // Relationships
            builder.HasMany(b => b.Payments)
                .WithOne(p => p.Billing)
                .HasForeignKey(p => p.BillingId)
                .OnDelete(DeleteBehavior.Restrict);
        }   
    }
}
