using BloodDonationSystem.Data;
using BloodDonationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public static class RewardDbSeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

                // Only seed if table is empty
                if (await context.Rewards.AnyAsync())
                {
                    Console.WriteLine("⏭ Rewards already seeded, skipping.");
                    return;
                }

                var defaultRewards = new List<Reward>
                {
                    new Reward
                    {
                        Title          = "Free Medical Checkup",
                        Description    = "Get a free basic health checkup at participating clinics.",
                        PointsRequired = 50,
                        IsAvailable    = true,
                        CreatedAt      = DateTime.UtcNow,
                    },
                    new Reward
                    {
                        Title          = "Pharmacy Discount",
                        Description    = "20% discount at partner pharmacies on your next purchase.",
                        PointsRequired = 100,
                        IsAvailable    = true,
                        CreatedAt      = DateTime.UtcNow,
                    },
                    new Reward
                    {
                        Title          = "Blood Test Package",
                        Description    = "Complete blood test panel including CBC and metabolic panel.",
                        PointsRequired = 150,
                        IsAvailable    = true,
                        CreatedAt      = DateTime.UtcNow,
                    },
                    new Reward
                    {
                        Title          = "Hospital Priority Service",
                        Description    = "Skip the queue at partner hospitals for your next visit.",
                        PointsRequired = 200,
                        IsAvailable    = true,
                        CreatedAt      = DateTime.UtcNow,
                    },
                    new Reward
                    {
                        Title          = "Full Health Package",
                        Description    = "Comprehensive health screening including blood work, ECG, and consultation.",
                        PointsRequired = 250,
                        IsAvailable    = true,
                        CreatedAt      = DateTime.UtcNow,
                    },
                };

                await context.Rewards.AddRangeAsync(defaultRewards);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Seeded {defaultRewards.Count} default rewards.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Reward seeding failed: {ex.Message}");
                throw;
            }
        }
    }
}
