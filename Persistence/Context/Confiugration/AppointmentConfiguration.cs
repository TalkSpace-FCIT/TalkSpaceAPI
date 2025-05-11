using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Confiugration
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.Property(a => a.VisitType)
                .IsRequired()
                .HasConversion<string>(); // Store enum as string in the database

            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(a => a.StatusUpdatedOn)
                .IsRequired();

            builder.HasOne(a => a.MedicalRecord)
                .WithOne(m => m.Appointment)
                .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsMany(a => a.StatusHistory, sh =>
            {
                sh.WithOwner().HasForeignKey("AppointmentId");
                sh.Property(h => h.OldStatus)
                    .IsRequired()
                    .HasConversion<string>();
                sh.Property(h => h.NewStatus)
                    .IsRequired()
                    .HasConversion<string>();
                sh.Property(h => h.ChangedOn)
                    .IsRequired();
                sh.ToTable("AppointmentStatusHistories");
            });
        }
    }
}
