using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Configure the discriminator column
            builder.HasDiscriminator<string>("Discriminator")
                   .HasValue<AppUser>("AppUser")
                   .HasValue<Doctor>("Doctor")
                   .HasValue<Patient>("Patient");

            // Configure properties for AppUser
            builder.Property(e => e.FullName).IsRequired();
            builder.Property(e => e.Discriminator).IsRequired();

            // Configure Doctor-specific properties
            builder.HasOne<Doctor>()
                   .WithOne()
                   .HasForeignKey<Doctor>(d => d.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            // Configure Patient-specific properties
            builder.HasOne<Patient>()
                   .WithOne()
                   .HasForeignKey<Patient>(p => p.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            // Receptionist has no additional properties, so no extra configuration is needed
        }
    }
}
