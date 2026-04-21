using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public static class AuthDbContextSeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // ── Seed Roles ───────────────────────────────────────────────
                string[] roles = { "App Admin", "Hospital Admin", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(role));
                        if (!result.Succeeded)
                            throw new Exception($"Failed to create role '{role}': " +
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                Console.WriteLine("✅ Roles seeded.");

                // ── Seed Users ───────────────────────────────────────────────
                var usersToSeed = new[]
                {
                    new
                    {
                        FullName    = "System Admin",
                        Email       = "admin@app.com",
                        Password    = "Admin@123456",
                        PhoneNumber = "01000000000",
                        Age         = 30,
                        Gender      = Gender.Male,
                        Address     = "Cairo, Egypt",
                        NationalId  = "00000000000001",
                        Role        = "App Admin",
                    },
                    new
                    {
                        FullName    = "Hospital Admin",
                        Email       = "hospitaladmin@app.com",
                        Password    = "HospAdmin@123456",
                        PhoneNumber = "01000000001",
                        Age         = 35,
                        Gender      = Gender.Male,
                        Address     = "Alexandria, Egypt",
                        NationalId  = "00000000000002",
                        Role        = "Hospital Admin",
                    },
                    new
                    {
                        FullName    = "Default User",
                        Email       = "user@app.com",
                        Password    = "User@123456",
                        PhoneNumber = "01000000002",
                        Age         = 25,
                        Gender      = Gender.Female,
                        Address     = "Giza, Egypt",
                        NationalId  = "00000000000003",
                        Role        = "User",
                    },
                };

                foreach (var seed in usersToSeed)
                {
                    var existing = await userManager.FindByEmailAsync(seed.Email);
                    if (existing is not null)
                    {
                        Console.WriteLine($"⏭ User '{seed.Email}' already exists, skipping.");
                        continue;
                    }

                    var user = new ApplicationUser
                    {
                        FullName = seed.FullName,
                        Email = seed.Email,
                        UserName = seed.Email,
                        PhoneNumber = seed.PhoneNumber,
                        Age = seed.Age,
                        Gender = seed.Gender,
                        Address = seed.Address,
                        NationalId = seed.NationalId,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var createResult = await userManager.CreateAsync(user, seed.Password);
                    if (!createResult.Succeeded)
                        throw new Exception($"Failed to create user '{seed.Email}': " +
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));

                    var roleResult = await userManager.AddToRoleAsync(user, seed.Role);
                    if (!roleResult.Succeeded)
                        throw new Exception($"Failed to assign role to '{seed.Email}': " +
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));

                    Console.WriteLine($"✅ Seeded user '{seed.Email}' with role '{seed.Role}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Seeding failed: {ex.Message}");
                throw;
            }
        }
    }
}
