using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Context;
using Serilog;

namespace Persistence.DbInitialization
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                Log.Information("Applying pending migrations...");
                await context.Database.MigrateAsync();

                Log.Information("Seeding database...");
                await SeedRolesAsync(roleManager);
                await SeedDefaultDoctorAsync(userManager, roleManager);
                Log.Information("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while migrating or seeding the database");
                throw;
            }

        }

        private static async Task SeedDefaultDoctorAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var doctorEmail = Constants.DoctorEmail;
            var doctorUser = await userManager.FindByEmailAsync(doctorEmail);

            if (doctorUser is null)
            {
                doctorUser = new Doctor
                {
                    FullName = "Dr. Jane Smith",
                    Email = doctorEmail,
                    UserName = doctorEmail,
                    Bio = "Licensed psychotherapist focused on anxiety, depression, and trauma recovery.",
                    Specialization = "Psychotherapist",
                    Discriminator = "Doctor",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(doctorUser, Constants.DoctorPassword);

                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to seed default doctor: {string.Join(", ", result.Errors)}");
                }
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = Constants.Roles;
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}