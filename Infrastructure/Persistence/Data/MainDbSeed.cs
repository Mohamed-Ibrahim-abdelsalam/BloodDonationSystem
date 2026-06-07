using BloodDonationSystem.Data;
using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Enums;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Data
{
    /// <summary>
    /// Seeds users, inventories, blood requests, donations, scans, user rewards,
    /// and inventory logs.
    ///
    /// DOES NOT seed:  Hospitals   (already added by the backend)
    ///                 Rewards     (already added by the backend)
    ///                 Notifications (removed from the system)
    /// </summary>
    public static class MainDbSeed
    {
        // ── Existing hospital IDs (already in DB) ─────────────────────────────
        private const int H_CairoUniversity = 1;   // Cairo University Hospital
        private const int H_AbuElReesh = 2;   // Abu El Reesh Children Hospital
        private const int H_AinShams = 3;   // Ain Shams University Hospital
        private const int H_AlAgouza = 4;   // Al Agouza Hospital
        private const int H_AlDemerdash = 5;   // Al Demerdash Hospital
        private const int H_AlGalaa = 6;   // Al Galaa Military Hospital
        private const int H_AlHaram = 7;   // Al Haram Hospital
        private const int H_AlSalam = 8;   // Al Salam International Hospital
        private const int H_Cleopatra = 9;   // Cleopatra Hospital
        private const int H_ElSahel = 10;   // El Sahel Teaching Hospital
        private const int H_ElShorouq = 11;   // El Shorouq Hospital
        private const int H_Heliopolis = 12;   // Heliopolis Hospital
        private const int H_IntlMedical = 13;   // International Medical Center
        private const int H_KobryElKobba = 14;   // Kobry El Kobba Military Hospital
        private const int H_MaadiMilitary = 15;   // Maadi Military Hospital
        private const int H_MisrIntl = 16;   // Misr International Hospital
        private const int H_NationalCancer = 17;   // National Cancer Institute
        private const int H_Oct6University = 18;   // October 6 University Hospital
        private const int H_SalamCity = 19;   // Salam City Hospital
        private const int H_Shubra = 20;   // Shubra El Kheima Hospital
        private const int H_WadiElNeel = 21;   // Wadi El Neel Hospital
        private const int H_ZayedSpecialized = 22;   // Zayed Specialized Hospital

        // ── Existing reward IDs (already in DB) ───────────────────────────────
        private const int R_FreeMedicalCheckup = 1;   // 50  pts
        private const int R_PharmacyDiscount = 2;   // 100 pts
        private const int R_BloodTestPackage = 3;   // 150 pts
        private const int R_HospitalPriorityService = 4;   // 200 pts
        private const int R_FullHealthPackage = 5;   // 250 pts

        // ── Short blood-type format for HospitalInventory & InventoryLog ──────
        private static string Short(BloodType bt) => bt switch
        {
            BloodType.A_Positive => "A+",
            BloodType.A_Negative => "A-",
            BloodType.B_Positive => "B+",
            BloodType.B_Negative => "B-",
            BloodType.AB_Positive => "AB+",
            BloodType.AB_Negative => "AB-",
            BloodType.O_Positive => "O+",
            BloodType.O_Negative => "O-",
            _ => "O+",
        };

        // ── Entry point ───────────────────────────────────────────────────────
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // ═════════════════════════════════════════════════════════════
                // 1.  HOSPITAL ADMINS  (6 admins — each linked to a hospital)
                // ═════════════════════════════════════════════════════════════

                var hospAdminSeeds = new[]
                {
                    new { FullName="Ahmed Mohamed El-Sayed",  Email="admin.cairo.uni@hospital.com",   Password="HospAdmin@101", Phone="01111000101", Age=42, Gender=Gender.Male,   NatId="29801012601101", HospId=H_CairoUniversity,  Address="Cairo, Egypt"          },
                    new { FullName="Mona Ibrahim Ali",         Email="admin.ainshams@hospital.com",    Password="HospAdmin@102", Phone="01111000102", Age=38, Gender=Gender.Female, NatId="29601054102102", HospId=H_AinShams,         Address="Cairo, Egypt"          },
                    new { FullName="Khaled Abdel Rahman",      Email="admin.zayed@hospital.com",       Password="HospAdmin@103", Phone="01111000103", Age=45, Gender=Gender.Male,   NatId="29401023303103", HospId=H_ZayedSpecialized, Address="Giza, Egypt"           },
                    new { FullName="Sara Mahmoud Fahmy",       Email="admin.cleopatra@hospital.com",   Password="HospAdmin@104", Phone="01111000104", Age=36, Gender=Gender.Female, NatId="29801054504104", HospId=H_Cleopatra,        Address="Alexandria, Egypt"     },
                    new { FullName="Omar Hassan Taher",        Email="admin.oct6@hospital.com",        Password="HospAdmin@105", Phone="01111000105", Age=40, Gender=Gender.Male,   NatId="29601015605105", HospId=H_Oct6University,   Address="October City, Egypt"   },
                    new { FullName="Dina Wael Abdallah",       Email="admin.alsalam@hospital.com",     Password="HospAdmin@106", Phone="01111000106", Age=34, Gender=Gender.Female, NatId="29901026706106", HospId=H_AlSalam,          Address="Cairo, Egypt"          },
                };

                var adminIds = new List<string>();

                foreach (var s in hospAdminSeeds)
                {
                    var existing = await userManager.FindByEmailAsync(s.Email);
                    if (existing is not null)
                    {
                        adminIds.Add(existing.Id);
                        Console.WriteLine($"⏭  Hospital admin '{s.Email}' already exists.");
                        continue;
                    }

                    var user = new ApplicationUser
                    {
                        FullName = s.FullName,
                        Email = s.Email,
                        UserName = s.Email,
                        PhoneNumber = s.Phone,
                        Age = s.Age,
                        Gender = s.Gender,
                        BloodType = BloodType.O_Positive,
                        Address = s.Address,
                        NationalId = s.NatId,
                        HospitalId = s.HospId,
                        Role = Role.HospitalAdmin,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-5),
                        Points = 0,
                    };

                    var result = await userManager.CreateAsync(user, s.Password);
                    if (!result.Succeeded)
                        throw new Exception($"Failed to create hospital admin '{s.Email}': " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));

                    await userManager.AddToRoleAsync(user, "HospitalAdmin");
                    adminIds.Add(user.Id);
                    Console.WriteLine($"✅ Seeded hospital admin: {s.Email}");
                }

                // ═════════════════════════════════════════════════════════════
                // 2.  DONOR USERS  (10 donors)
                // ═════════════════════════════════════════════════════════════

                var donorSeeds = new[]
                {
                    new { FullName="Mohamed Ali Hassan",    Email="donor1@mail.com",  Password="Donor@11111", Phone="01200000011", Age=28, Gender=Gender.Male,   BT=BloodType.A_Positive,  NatId="29701013101011", Address="Nasr City, Cairo",        Points=150 },
                    new { FullName="Fatma Ahmed Salem",     Email="donor2@mail.com",  Password="Donor@22222", Phone="01200000022", Age=24, Gender=Gender.Female, BT=BloodType.B_Negative,  NatId="30001024502022", Address="Mohandessin, Giza",        Points=100 },
                    new { FullName="Yousef Mahmoud Kamal",  Email="donor3@mail.com",  Password="Donor@33333", Phone="01200000033", Age=32, Gender=Gender.Male,   BT=BloodType.O_Positive,  NatId="29201013503033", Address="Zamalek, Cairo",           Points=200 },
                    new { FullName="Nourhan Samir Obeid",   Email="donor4@mail.com",  Password="Donor@44444", Phone="01200000044", Age=26, Gender=Gender.Female, BT=BloodType.AB_Positive, NatId="29801034904044", Address="Smouha, Alexandria",       Points=50  },
                    new { FullName="Hossam Adel Mansour",   Email="donor5@mail.com",  Password="Donor@55555", Phone="01200000055", Age=35, Gender=Gender.Male,   BT=BloodType.A_Negative,  NatId="28901015505055", Address="Maadi, Cairo",             Points=300 },
                    new { FullName="Rana Tarek Rashid",     Email="donor6@mail.com",  Password="Donor@66666", Phone="01200000066", Age=22, Gender=Gender.Female, BT=BloodType.O_Negative,  NatId="30201016606066", Address="Cleopatra, Alexandria",    Points=75  },
                    new { FullName="Karim Saeed Gad",       Email="donor7@mail.com",  Password="Donor@77777", Phone="01200000077", Age=30, Gender=Gender.Male,   BT=BloodType.B_Positive,  NatId="29401037107077", Address="Dokki, Giza",              Points=125 },
                    new { FullName="Mariam Walid Fathy",    Email="donor8@mail.com",  Password="Donor@88888", Phone="01200000088", Age=27, Gender=Gender.Female, BT=BloodType.AB_Negative, NatId="29701018208088", Address="Heliopolis, Cairo",        Points=0   },
                    new { FullName="Amr Sherif Barakat",    Email="donor9@mail.com",  Password="Donor@99999", Phone="01200000099", Age=31, Gender=Gender.Male,   BT=BloodType.O_Positive,  NatId="29301039309099", Address="October 6 City, Giza",    Points=225 },
                    new { FullName="Yasmin Nabil Fouad",    Email="donor10@mail.com", Password="Donor@10101", Phone="01200000110", Age=25, Gender=Gender.Female, BT=BloodType.A_Positive,  NatId="29901040010110", Address="Mansoura, Dakahlia",       Points=80  },
                };

                var donorIds = new List<string>();

                foreach (var s in donorSeeds)
                {
                    var existing = await userManager.FindByEmailAsync(s.Email);
                    if (existing is not null)
                    {
                        donorIds.Add(existing.Id);
                        Console.WriteLine($"⏭  Donor '{s.Email}' already exists.");
                        continue;
                    }

                    var user = new ApplicationUser
                    {
                        FullName = s.FullName,
                        Email = s.Email,
                        UserName = s.Email,
                        PhoneNumber = s.Phone,
                        Age = s.Age,
                        Gender = s.Gender,
                        BloodType = s.BT,
                        Address = s.Address,
                        NationalId = s.NatId,
                        Role = Role.User,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2),
                        Points = s.Points,
                    };

                    var result = await userManager.CreateAsync(user, s.Password);
                    if (!result.Succeeded)
                        throw new Exception($"Failed to create donor '{s.Email}': " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));

                    await userManager.AddToRoleAsync(user, "User");
                    donorIds.Add(user.Id);
                    Console.WriteLine($"✅ Seeded donor: {s.Email}");
                }

                // Safety index helpers
                string D(int i) => donorIds.Count > i ? donorIds[i] : donorIds[0];
                string A(int i) => adminIds.Count > i ? adminIds[i] : adminIds[0];

                // ═════════════════════════════════════════════════════════════
                // 3.  HOSPITAL INVENTORIES  (8 blood types × 10 hospitals = 80)
                // ═════════════════════════════════════════════════════════════

                if (!await context.HospitalInventories.AnyAsync())
                {
                    // Short blood type → base quantity
                    var bts = new Dictionary<string, int>
                    {
                        { "A+", 18 }, { "A-",  4 },
                        { "B+", 12 }, { "B-",  3 },
                        { "AB+", 6 }, { "AB-", 2 },
                        { "O+", 20 }, { "O-",  5 },
                    };

                    // (hospitalId, adminIndex, scale factor)
                    var hospData = new[]
                    {
                        (H_CairoUniversity,  0, 1.5),
                        (H_AinShams,         1, 1.3),
                        (H_ZayedSpecialized, 2, 1.2),
                        (H_Cleopatra,        3, 0.9),
                        (H_Oct6University,   4, 0.8),
                        (H_AlSalam,          5, 1.0),
                        (H_MisrIntl,         0, 0.7),
                        (H_Heliopolis,       1, 0.6),
                        (H_ElShorouq,        2, 0.5),
                        (H_AlGalaa,          3, 1.1),
                    };

                    var inventories = new List<HospitalInventory>();
                    foreach (var (hospId, adminIdx, scale) in hospData)
                    {
                        foreach (var bt in bts)
                        {
                            inventories.Add(new HospitalInventory
                            {
                                HospitalId = hospId,
                                BloodType = bt.Key,
                                Quantity = Math.Max(0, (int)(bt.Value * scale)),
                                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                                UpdatedByAdminId = A(adminIdx),
                                UpdateSource = "Admin",
                            });
                        }
                    }

                    await context.HospitalInventories.AddRangeAsync(inventories);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Seeded {inventories.Count} inventory records.");
                }
                else
                {
                    Console.WriteLine("⏭  HospitalInventories already seeded.");
                }

                // ═════════════════════════════════════════════════════════════
                // 6.  DONATION SCANS  (one scan per Confirmed donation)
                // ═════════════════════════════════════════════════════════════

                if (!await context.DonationScans.AnyAsync())
                {
                    var confirmed = await context.Donations
                        .Where(d => d.Status == DonationStatus.Confirmed)
                        .ToListAsync();

                    var scans = confirmed.Select((don, idx) => new DonationScan
                    {
                        DonationId = don.Id,
                        ScannedByHospitalAdminId = A(idx % adminIds.Count),
                        ScanTime = don.ConfirmedAt ?? DateTime.UtcNow.AddDays(-1),
                    }).ToList();

                    if (scans.Any())
                    {
                        await context.DonationScans.AddRangeAsync(scans);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✅ Seeded {scans.Count} donation scans.");
                    }
                }
                else
                {
                    Console.WriteLine("⏭  DonationScans already seeded.");
                }

                // ═════════════════════════════════════════════════════════════
                // 7.  PICKUP SCANS  (one scan per Completed request)
                // ═════════════════════════════════════════════════════════════

                if (!await context.PickupScans.AnyAsync())
                {
                    var completed = await context.BloodRequests
                        .Where(r => r.Status == BloodRequestStatus.Completed)
                        .ToListAsync();

                    var pickups = completed.Select((req, idx) => new PickupScan
                    {
                        BloodRequestId = req.Id,
                        // Must be the request owner — not a random donor
                        ScannedByUserId = req.RequestedByUserId,
                        ScanTime = req.NeededBy?.AddDays(-1) ?? DateTime.UtcNow.AddDays(-6),
                    }).ToList();

                    if (pickups.Any())
                    {
                        await context.PickupScans.AddRangeAsync(pickups);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✅ Seeded {pickups.Count} pickup scans.");
                    }
                }
                else
                {
                    Console.WriteLine("⏭  PickupScans already seeded.");
                }

                // ═════════════════════════════════════════════════════════════
                // 8.  USER REWARDS  (uses existing reward IDs 1-5)
                // ═════════════════════════════════════════════════════════════

                if (!await context.UserRewards.AnyAsync())
                {
                    var userRewards = new List<UserReward>();

                    void Add(int donorIdx, int rewardId, int ptsUsed, UserRewardStatus status, int daysAgo)
                    {
                        if (donorIdx >= donorIds.Count) return;
                        userRewards.Add(new UserReward
                        {
                            UserId = donorIds[donorIdx],
                            RewardId = rewardId,
                            PointsUsed = ptsUsed,
                            Status = status,
                            RedeemedAt = DateTime.UtcNow.AddDays(-daysAgo),
                        });
                    }

                    // donor5  (300 pts) → Full Health Package      (250 pts)
                    Add(4, R_FullHealthPackage, 250, UserRewardStatus.Used, 10);
                    // donor3  (200 pts) → Hospital Priority Service (200 pts)
                    Add(2, R_HospitalPriorityService, 200, UserRewardStatus.Used, 5);
                    // donor9  (225 pts) → Hospital Priority Service (200 pts)
                    Add(8, R_HospitalPriorityService, 200, UserRewardStatus.Unused, 2);
                    // donor1  (150 pts) → Blood Test Package        (150 pts)
                    Add(0, R_BloodTestPackage, 150, UserRewardStatus.Unused, 1);
                    // donor2  (100 pts) → Pharmacy Discount         (100 pts)
                    Add(1, R_PharmacyDiscount, 100, UserRewardStatus.Unused, 3);
                    // donor7  (125 pts) → Pharmacy Discount         (100 pts)
                    Add(6, R_PharmacyDiscount, 100, UserRewardStatus.Used, 6);
                    // donor10 ( 80 pts) → Free Medical Checkup      ( 50 pts)
                    Add(9, R_FreeMedicalCheckup, 50, UserRewardStatus.Unused, 0);
                    // donor6  ( 75 pts) → Free Medical Checkup      ( 50 pts)
                    Add(5, R_FreeMedicalCheckup, 50, UserRewardStatus.Used, 4);

                    if (userRewards.Any())
                    {
                        await context.UserRewards.AddRangeAsync(userRewards);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✅ Seeded {userRewards.Count} user rewards.");
                    }
                }
                else
                {
                    Console.WriteLine("⏭  UserRewards already seeded.");
                }


                // ═════════════════════════════════════════════════════════════
                // 9.  INVENTORY LOGS
                // ═════════════════════════════════════════════════════════════

                if (!await context.InventoryLogs.AnyAsync())
                {
                    var invList = await context.HospitalInventories.ToListAsync();
                    var confirmedDonations = await context.Donations
                        .Where(d => d.Status == DonationStatus.Confirmed && d.HospitalId != null)
                        .ToListAsync();

                    var logs = new List<InventoryLog>();

                    // One log per confirmed donation
                    foreach (var don in confirmedDonations)
                    {
                        var shortBT = Short(don.BloodType);
                        var inv = invList.FirstOrDefault(i =>
                            i.HospitalId == don.HospitalId && i.BloodType == shortBT);
                        if (inv is null) continue;

                        logs.Add(new InventoryLog
                        {
                            HospitalInventoryId = inv.Id,
                            HospitalId = don.HospitalId!.Value,
                            BloodType = shortBT,
                            DonationId = don.Id,
                            BloodRequestId = null,
                            ChangedByAdminId = adminIds.Count > 0 ? adminIds[0] : null,
                            Source = "Donation",
                            ChangeAmount = 1,
                            QuantityAfter = inv.Quantity,
                            Notes = "Blood bag received from confirmed donation",
                            ChangedAt = don.ConfirmedAt ?? DateTime.UtcNow,
                        });
                    }

                    // Manual adjustment logs
                    HospitalInventory? Inv(int hospId, string bt) =>
                        invList.FirstOrDefault(i => i.HospitalId == hospId && i.BloodType == bt);

                    var invA = Inv(H_CairoUniversity, "O+");
                    if (invA is not null)
                        logs.Add(new InventoryLog
                        {
                            HospitalInventoryId = invA.Id,
                            HospitalId = H_CairoUniversity,
                            BloodType = "O+",
                            ChangedByAdminId = A(0),
                            Source = "ManualAdjustment",
                            ChangeAmount = 10,
                            QuantityAfter = invA.Quantity + 10,
                            Notes = "External blood bank transfer received",
                            ChangedAt = DateTime.UtcNow.AddDays(-6),
                        });

                    var invB = Inv(H_Oct6University, "A+");
                    if (invB is not null)
                        logs.Add(new InventoryLog
                        {
                            HospitalInventoryId = invB.Id,
                            HospitalId = H_Oct6University,
                            BloodType = "A+",
                            ChangedByAdminId = A(4),
                            Source = "ManualAdjustment",
                            ChangeAmount = 6,
                            QuantityAfter = invB.Quantity + 6,
                            Notes = "Initial stock entry upon hospital registration",
                            ChangedAt = DateTime.UtcNow.AddDays(-3),
                        });

                    if (logs.Any())
                    {
                        await context.InventoryLogs.AddRangeAsync(logs);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✅ Seeded {logs.Count} inventory logs.");
                    }
                }
                else
                {
                    Console.WriteLine("⏭  InventoryLogs already seeded.");
                }

                Console.WriteLine("🎉 MainDbSeed completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MainDbSeed failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
    }
}