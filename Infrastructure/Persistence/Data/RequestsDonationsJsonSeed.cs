using BloodDonationSystem.Data;
using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using Microsoft.AspNetCore.Identity;
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
    public static class RequestsDonationsJsonSeed
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var seedDir = Path.GetDirectoryName(
                    typeof(RequestsDonationsJsonSeed).Assembly.Location)!;

                // ── 1. BLOOD REQUESTS ─────────────────────────────────────────
                if (!await context.BloodRequests.AnyAsync())
                {
                    var reqPath = Path.Combine(seedDir, "requests_seed.json");
                    Console.WriteLine($"📂 Requests path: {reqPath}");

                    if (!File.Exists(reqPath))
                    {
                        Console.WriteLine($"❌ File not found: {reqPath}");
                        return;
                    }

                    var reqJson = await File.ReadAllTextAsync(reqPath);
                    var reqPayload = JsonSerializer.Deserialize<RequestSeedPayload>(reqJson, options)
                        ?? throw new InvalidOperationException("Failed to parse requests_seed.json.");

                    var reqEmailToId = await ResolveEmailsAsync(userManager,
                        reqPayload.BloodRequests.Select(r => r.RequestedByEmail));

                    if (reqEmailToId.Count == 0)
                    {
                        Console.WriteLine("❌ No users found — BloodRequests skipped.");
                        return;
                    }

                    var requests = reqPayload.BloodRequests
                        .Where(r => reqEmailToId.ContainsKey(r.RequestedByEmail))
                        .Select(r => new BloodRequest
                        {
                            RequestedByUserId = reqEmailToId[r.RequestedByEmail],
                            HospitalId = r.HospitalId,
                            HospitalName = r.HospitalName,
                            HospitalLocation = r.HospitalLocation,
                            Latitude = r.Latitude,
                            Longitude = r.Longitude,
                            BloodType = (BloodType)r.BloodType,
                            Quantity = r.Quantity,
                            Priority = (RequestPriority)r.Priority,
                            Status = (BloodRequestStatus)r.Status,
                            IsBloodReceived = r.IsBloodReceived,
                            NeededBy = r.NeededBy,
                            CreatedAt = r.CreatedAt,
                        }).ToList();

                    await context.BloodRequests.AddRangeAsync(requests);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Seeded {requests.Count} blood requests.");
                }
                else
                {
                    Console.WriteLine("⏭  BloodRequests already seeded.");
                }

                // ── 2. DONATIONS ──────────────────────────────────────────────
                if (!await context.Donations.AnyAsync())
                {
                    var donPath = Path.Combine(seedDir, "donations_seed.json");
                    Console.WriteLine($"📂 Donations path: {donPath}");

                    if (!File.Exists(donPath))
                    {
                        Console.WriteLine($"❌ File not found: {donPath}");
                        return;
                    }

                    var donJson = await File.ReadAllTextAsync(donPath);
                    var donPayload = JsonSerializer.Deserialize<DonationSeedPayload>(donJson, options)
                        ?? throw new InvalidOperationException("Failed to parse donations_seed.json.");

                    var donEmailToId = await ResolveEmailsAsync(userManager,
                        donPayload.Donations.Select(d => d.DonorEmail));

                    if (donEmailToId.Count == 0)
                    {
                        Console.WriteLine("❌ No users found — Donations skipped.");
                        return;
                    }

                    var donations = donPayload.Donations
                        .Where(d => donEmailToId.ContainsKey(d.DonorEmail))
                        .Select(d => new Donation
                        {
                            DonorUserId = donEmailToId[d.DonorEmail],
                            BloodRequestId = d.BloodRequestId,
                            HospitalId = d.HospitalId,
                            BloodType = (BloodType)d.BloodType,
                            Age = d.Age,
                            Weight = d.Weight,
                            HasTattoo = d.HasTattoo,
                            LastDonationDate = d.LastDonationDate,
                            Address = d.Address,
                            MedicalCondition = d.MedicalCondition,
                            Status = (DonationStatus)d.Status,
                            CreatedAt = d.CreatedAt,
                            ConfirmedAt = d.ConfirmedAt,
                        }).ToList();

                    await context.Donations.AddRangeAsync(donations);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Seeded {donations.Count} donations.");
                }
                else
                {
                    Console.WriteLine("⏭  Donations already seeded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RequestsDonationsJsonSeed failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private static async Task<Dictionary<string, string>> ResolveEmailsAsync(
            UserManager<ApplicationUser> userManager,
            IEnumerable<string> emails)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var email in emails.Distinct())
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user is not null)
                    map[email] = user.Id;
                else
                    Console.WriteLine($"  ⚠️  User not found: {email}");
            }
            Console.WriteLine($"  ✅ Resolved {map.Count} users.");
            return map;
        }

        // ── JSON DTOs ─────────────────────────────────────────────────────────
        private class RequestSeedPayload
        {
            [JsonPropertyName("BloodRequests")]
            public List<RequestRecord> BloodRequests { get; set; } = new();
        }

        private class DonationSeedPayload
        {
            [JsonPropertyName("Donations")]
            public List<DonationRecord> Donations { get; set; } = new();
        }

        private class RequestRecord
        {
            [JsonPropertyName("Id")] public int Id { get; set; }
            [JsonPropertyName("RequestedByEmail")] public string RequestedByEmail { get; set; } = "";
            [JsonPropertyName("HospitalId")] public int HospitalId { get; set; }
            [JsonPropertyName("HospitalName")] public string HospitalName { get; set; } = "";
            [JsonPropertyName("HospitalLocation")] public string HospitalLocation { get; set; } = "";
            [JsonPropertyName("Latitude")] public double? Latitude { get; set; }
            [JsonPropertyName("Longitude")] public double? Longitude { get; set; }
            [JsonPropertyName("BloodType")] public int BloodType { get; set; }
            [JsonPropertyName("Quantity")] public int Quantity { get; set; }
            [JsonPropertyName("Priority")] public int Priority { get; set; }
            [JsonPropertyName("Status")] public int Status { get; set; }
            [JsonPropertyName("IsBloodReceived")] public bool IsBloodReceived { get; set; }
            [JsonPropertyName("NeededBy")] public DateTime? NeededBy { get; set; }
            [JsonPropertyName("CreatedAt")] public DateTime CreatedAt { get; set; }
        }

        private class DonationRecord
        {
            [JsonPropertyName("Id")] public int Id { get; set; }
            [JsonPropertyName("DonorEmail")] public string DonorEmail { get; set; } = "";
            [JsonPropertyName("BloodRequestId")] public int? BloodRequestId { get; set; }
            [JsonPropertyName("HospitalId")] public int HospitalId { get; set; }
            [JsonPropertyName("BloodType")] public int BloodType { get; set; }
            [JsonPropertyName("Age")] public int Age { get; set; }
            [JsonPropertyName("Weight")] public double Weight { get; set; }
            [JsonPropertyName("HasTattoo")] public bool HasTattoo { get; set; }
            [JsonPropertyName("LastDonationDate")] public DateTime? LastDonationDate { get; set; }
            [JsonPropertyName("Address")] public string Address { get; set; } = "";
            [JsonPropertyName("MedicalCondition")] public string MedicalCondition { get; set; } = "False";
            [JsonPropertyName("Status")] public int Status { get; set; }
            [JsonPropertyName("CreatedAt")] public DateTime CreatedAt { get; set; }
            [JsonPropertyName("ConfirmedAt")] public DateTime? ConfirmedAt { get; set; }
        }
    }
}
