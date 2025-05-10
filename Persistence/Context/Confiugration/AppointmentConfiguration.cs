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
    public class AppointmentConfiguration: IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment>builder)
        {
            builder.Property(a=>a.VisitType)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(a=>a.Status)
             .IsRequired()
             .HasConversion<string>();
            builder.HasOne(a => a.MedicalRecord)
                .WithOne(m => m.Appointment)
                .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
