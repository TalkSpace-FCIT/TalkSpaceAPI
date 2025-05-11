using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context.Confiugration
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
        {
            public void Configure(EntityTypeBuilder<Patient> builder)
            {
                builder.Property(p => p.DateOfBirth)
                    .IsRequired();

                builder.Property(p => p.Gender)
                    .IsRequired();

                // Relationships
                builder.HasMany(p => p.MedicalRecords)
                    .WithOne(m => m.Patient)
                    .HasForeignKey(m => m.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(p => p.Appointments)
                    .WithOne(a => a.Patient)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

}
