using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Context.Confiugration
{
    public class ReportConfiguration:IEntityTypeConfiguration<Report>
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
            // add a one to many relationship with receptionist
            //builder.HasOne(r => r.GeneratedBy)
            //.WithMany(r => r.GeneratedReports)
            //.HasForeignKey(r => r.ReceptionistId)
            //.OnDelete(DeleteBehavior.Restrict);
        }
    }

}
