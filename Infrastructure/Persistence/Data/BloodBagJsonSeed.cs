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
        private const string ResourceName =
            "Persistence.Data.blood_bags_seed.json";

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                if (await context.BloodBags.AnyAsync())
                {
                    Console.WriteLine("⏭  BloodBags already seeded.");
                    return;
                }

                // ── Read JSON from embedded resource ──────────────────────────
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(ResourceName)
                    ?? throw new FileNotFoundException(
                        $"Embedded resource '{ResourceName}' not found. " +
                        "Ensure blood_bags_seed.json has Build Action = EmbeddedResource.");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                var payload = await JsonSerializer.DeserializeAsync<BloodBagSeedPayload>(
                    stream, options)
                    ?? throw new InvalidOperationException(
                        "Failed to deserialize blood_bags_seed.json.");

                if (payload.BloodBags is null || !payload.BloodBags.Any())
                {
                    Console.WriteLine("⚠️  blood_bags_seed.json contains no rows.");
                    return;
                }

                // ── Map JSON records → BloodBag entities ──────────────────────
                var bags = payload.BloodBags.Select(r => new BloodBag
                {
                    DonationId = r.DonationId,
                    HospitalId = r.HospitalId,
                    BloodType = (BloodDonationSystem.Enums.BloodType)r.BloodType,
                    Status = (BloodBagStatus)r.Status,
                    CreatedAt = r.CreatedAt,
                    ExpiryDate = r.ExpiryDate,
                    WithdrawnAt = r.WithdrawnAt,
                }).ToList();

                await context.BloodBags.AddRangeAsync(bags);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Seeded {bags.Count} blood bags from JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BloodBag seeding failed: {ex.Message}");
                throw;
            }
        }

        // ── JSON shape DTOs (private — only used during seeding) ─────────────

        private class BloodBagSeedPayload
        {
            [JsonPropertyName("BloodBags")]
            public List<BloodBagSeedRecord>? BloodBags { get; set; }
        }

        private class BloodBagSeedRecord
        {
            [JsonPropertyName("Id")]
            public int Id { get; set; }

            [JsonPropertyName("DonationId")]
            public int DonationId { get; set; }

            [JsonPropertyName("HospitalId")]
            public int HospitalId { get; set; }

            /// <summary>Integer matching BloodType enum (1=A_Positive … 8=O_Negative).</summary>
            [JsonPropertyName("BloodType")]
            public int BloodType { get; set; }

            /// <summary>0=Available, 1=Withdrawn</summary>
            [JsonPropertyName("Status")]
            public int Status { get; set; }

            [JsonPropertyName("CreatedAt")]
            public DateTime CreatedAt { get; set; }

            [JsonPropertyName("ExpiryDate")]
            public DateTime ExpiryDate { get; set; }

            [JsonPropertyName("WithdrawnAt")]
            public DateTime? WithdrawnAt { get; set; }
        }
    }
}
