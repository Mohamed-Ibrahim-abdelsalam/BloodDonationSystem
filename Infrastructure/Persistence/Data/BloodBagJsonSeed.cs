using BloodDonationSystem.Data;
using DomainLayer.Enums;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public static class BloodBagJsonSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context, string contentRootPath)
        {
                try
                {
                    if (await context.BloodBags.AnyAsync())
                    {
                        Console.WriteLine("⏭  BloodBags already seeded.");
                        return;
                    }
    
                  var filePath = Path.Combine(contentRootPath, "SeedData", "blood_bags_seed.json");

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"❌ File not found: {filePath}");
                    return;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var payload = JsonSerializer.Deserialize<BloodBagSeedPayload>(json, options)
                    ?? throw new InvalidOperationException("Failed to parse blood_bags_seed.json.");

                if (payload.BloodBags is null || !payload.BloodBags.Any())
                {
                    Console.WriteLine("⚠️  blood_bags_seed.json contains no rows.");
                    return;
                }

                var dbDonations = await context.Donations
                    .OrderBy(d => d.CreatedAt)
                    .Select(d => d.Id)
                    .ToListAsync();

                var donIdMap = new Dictionary<int, int>();

                for (int i = 0; i < dbDonations.Count && i < payload.BloodBags.Count; i++)
                {
                    donIdMap[i + 1] = dbDonations[i];
                }

                Console.WriteLine($"📋 Mapped {donIdMap.Count} donation IDs.");


                var bags = payload.BloodBags.Select(r => new BloodBag
                {
                    DonationId = donIdMap.ContainsKey(r.DonationId)
                          ? donIdMap[r.DonationId]
                          : r.DonationId,
                    HospitalId = r.HospitalId,
                    BloodType = (BloodDonationSystem.Enums.BloodType)r.BloodType,
                    Status = (BloodBagStatus)r.Status,
                    CreatedAt = r.CreatedAt,
                    ExpiryDate = r.ExpiryDate,
                    WithdrawnAt = r.WithdrawnAt,
                }).ToList();

                await context.BloodBags.AddRangeAsync(bags);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Seeded {bags.Count} blood bags.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BloodBag seeding failed: {ex.Message}");
                throw;
            }
        }

        private class BloodBagSeedPayload
        {
            [JsonPropertyName("BloodBags")]
            public List<BloodBagSeedRecord>? BloodBags { get; set; }
        }

        private class BloodBagSeedRecord
        {
            [JsonPropertyName("Id")] public int Id { get; set; }
            [JsonPropertyName("DonationId")] public int DonationId { get; set; }
            [JsonPropertyName("HospitalId")] public int HospitalId { get; set; }
            [JsonPropertyName("BloodType")] public int BloodType { get; set; }
            [JsonPropertyName("Status")] public int Status { get; set; }
            [JsonPropertyName("CreatedAt")] public DateTime CreatedAt { get; set; }
            [JsonPropertyName("ExpiryDate")] public DateTime ExpiryDate { get; set; }
            [JsonPropertyName("WithdrawnAt")] public DateTime? WithdrawnAt { get; set; }
        }
    }
}
