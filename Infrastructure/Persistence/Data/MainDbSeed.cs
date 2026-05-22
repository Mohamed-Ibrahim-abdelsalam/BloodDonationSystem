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
                // 4.  BLOOD REQUESTS  (10 requests — varied statuses)
                // ═════════════════════════════════════════════════════════════

                if (!await context.BloodRequests.AnyAsync())
                {
                    var requests = new List<BloodRequest>
                    {
                        new BloodRequest
                        {
                            RequestedByUserId = D(0),
                            HospitalId        = H_CairoUniversity,
                            HospitalName      = "Cairo University Hospital",
                            HospitalLocation  = "Kasr Al-Ainy Street, Cairo",
                            BloodType         = BloodType.A_Positive,
                            Quantity          = 2,
                            Priority          = RequestPriority.Emergency,
                            Status            = BloodRequestStatus.Fulfilled,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(-4),
                            CreatedAt         = DateTime.UtcNow.AddDays(-9),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(1),
                            HospitalId        = H_AinShams,
                            HospitalName      = "Ain Shams University Hospital",
                            HospitalLocation  = "Abbasia, Cairo",
                            BloodType         = BloodType.O_Negative,
                            Quantity          = 3,
                            Priority          = RequestPriority.Emergency,
                            Status            = BloodRequestStatus.Open,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(2),
                            CreatedAt         = DateTime.UtcNow.AddDays(-1),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(2),
                            HospitalId        = H_ZayedSpecialized,
                            HospitalName      = "Zayed Specialized Hospital",
                            HospitalLocation  = "Sheikh Zayed City, Giza",
                            BloodType         = BloodType.B_Positive,
                            Quantity          = 1,
                            Priority          = RequestPriority.Normal,
                            Status            = BloodRequestStatus.Completed,
                            IsBloodReceived   = true,
                            NeededBy          = DateTime.UtcNow.AddDays(-14),
                            CreatedAt         = DateTime.UtcNow.AddDays(-19),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(3),
                            HospitalId        = H_Cleopatra,
                            HospitalName      = "Cleopatra Hospital",
                            HospitalLocation  = "Heliopolis, Cairo",
                            BloodType         = BloodType.AB_Positive,
                            Quantity          = 2,
                            Priority          = RequestPriority.Normal,
                            Status            = BloodRequestStatus.Open,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(7),
                            CreatedAt         = DateTime.UtcNow.AddDays(-2),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(4),
                            HospitalId        = H_MisrIntl,
                            HospitalName      = "Misr International Hospital",
                            HospitalLocation  = "Dokki, Giza",
                            BloodType         = BloodType.O_Positive,
                            Quantity          = 4,
                            Priority          = RequestPriority.Emergency,
                            Status            = BloodRequestStatus.Closed,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(-28),
                            CreatedAt         = DateTime.UtcNow.AddDays(-33),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(5),
                            HospitalId        = H_Oct6University,
                            HospitalName      = "October 6 University Hospital",
                            HospitalLocation  = "Central Axis, October 6 City, Giza",
                            BloodType         = BloodType.A_Negative,
                            Quantity          = 1,
                            Priority          = RequestPriority.Normal,
                            Status            = BloodRequestStatus.Open,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(5),
                            CreatedAt         = DateTime.UtcNow.AddHours(-8),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(6),
                            HospitalId        = H_AlSalam,
                            HospitalName      = "Al Salam International Hospital",
                            HospitalLocation  = "Corniche El Nile, Cairo",
                            BloodType         = BloodType.B_Negative,
                            Quantity          = 2,
                            Priority          = RequestPriority.Emergency,
                            Status            = BloodRequestStatus.Open,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(1),
                            CreatedAt         = DateTime.UtcNow.AddHours(-10),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(7),
                            HospitalId        = H_Heliopolis,
                            HospitalName      = "Heliopolis Hospital",
                            HospitalLocation  = "Heliopolis, Cairo",
                            BloodType         = BloodType.AB_Negative,
                            Quantity          = 1,
                            Priority          = RequestPriority.Normal,
                            Status            = BloodRequestStatus.Completed,
                            IsBloodReceived   = true,
                            NeededBy          = DateTime.UtcNow.AddDays(-7),
                            CreatedAt         = DateTime.UtcNow.AddDays(-12),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(8),
                            HospitalId        = H_Oct6University,
                            HospitalName      = "October 6 University Hospital",
                            HospitalLocation  = "Central Axis, October 6 City, Giza",
                            BloodType         = BloodType.O_Positive,
                            Quantity          = 3,
                            Priority          = RequestPriority.Emergency,
                            Status            = BloodRequestStatus.Fulfilled,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(-1),
                            CreatedAt         = DateTime.UtcNow.AddDays(-6),
                        },
                        new BloodRequest
                        {
                            RequestedByUserId = D(9),
                            HospitalId        = H_CairoUniversity,
                            HospitalName      = "Cairo University Hospital",
                            HospitalLocation  = "Kasr Al-Ainy Street, Cairo",
                            BloodType         = BloodType.B_Positive,
                            Quantity          = 2,
                            Priority          = RequestPriority.Normal,
                            Status            = BloodRequestStatus.Open,
                            IsBloodReceived   = false,
                            NeededBy          = DateTime.UtcNow.AddDays(10),
                            CreatedAt         = DateTime.UtcNow.AddHours(-3),
                        },
                    };

                    await context.BloodRequests.AddRangeAsync(requests);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Seeded {requests.Count} blood requests.");
                }
                else
                {
                    Console.WriteLine("⏭  BloodRequests already seeded.");
                }

                // ═════════════════════════════════════════════════════════════
                // 5.  DONATIONS  (10 donations — varied statuses)
                // ═════════════════════════════════════════════════════════════

                if (!await context.Donations.AnyAsync())
                {
                    var dbReqs = await context.BloodRequests.OrderBy(r => r.Id).ToListAsync();
                    int? ReqId(int i) => dbReqs.Count > i ? dbReqs[i].Id : (int?)null;

                    var donations = new List<Donation>
                    {
                        // Request[0] Fulfilled — donor confirms donation
                        new Donation
                        {
                            DonorUserId      = D(2),
                            BloodRequestId   = ReqId(0),
                            HospitalId       = H_CairoUniversity,
                            BloodType        = BloodType.A_Positive,
                            Age = 32, Weight = 80, HasTattoo = false,
                            LastDonationDate = DateTime.UtcNow.AddMonths(-4),
                            Address          = "Zamalek, Cairo",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Confirmed,
                            CreatedAt        = DateTime.UtcNow.AddDays(-8),
                            ConfirmedAt      = DateTime.UtcNow.AddDays(-7),
                        },
                        // Request[2] Completed — confirmed
                        new Donation
                        {
                            DonorUserId      = D(4),
                            BloodRequestId   = ReqId(2),
                            HospitalId       = H_ZayedSpecialized,
                            BloodType        = BloodType.B_Positive,
                            Age = 35, Weight = 75, HasTattoo = false,
                            LastDonationDate = DateTime.UtcNow.AddMonths(-5),
                            Address          = "Maadi, Cairo",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Confirmed,
                            CreatedAt        = DateTime.UtcNow.AddDays(-18),
                            ConfirmedAt      = DateTime.UtcNow.AddDays(-17),
                        },
                        // Request[7] Completed — confirmed
                        new Donation
                        {
                            DonorUserId      = D(6),
                            BloodRequestId   = ReqId(7),
                            HospitalId       = H_Heliopolis,
                            BloodType        = BloodType.AB_Negative,
                            Age = 30, Weight = 70, HasTattoo = false,
                            LastDonationDate = null,
                            Address          = "Dokki, Giza",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Confirmed,
                            CreatedAt        = DateTime.UtcNow.AddDays(-11),
                            ConfirmedAt      = DateTime.UtcNow.AddDays(-10),
                        },
                        // Request[8] Fulfilled — confirmed
                        new Donation
                        {
                            DonorUserId      = D(8),
                            BloodRequestId   = ReqId(8),
                            HospitalId       = H_Oct6University,
                            BloodType        = BloodType.O_Positive,
                            Age = 31, Weight = 82, HasTattoo = false,
                            LastDonationDate = DateTime.UtcNow.AddMonths(-6),
                            Address          = "October 6 City, Giza",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Confirmed,
                            CreatedAt        = DateTime.UtcNow.AddDays(-5),
                            ConfirmedAt      = DateTime.UtcNow.AddDays(-4),
                        },
                        // General donations (no linked request)
                        new Donation
                        {
                            DonorUserId      = D(0),
                            BloodRequestId   = null,
                            HospitalId       = H_CairoUniversity,
                            BloodType        = BloodType.O_Positive,
                            Age = 28, Weight = 78, HasTattoo = false,
                            LastDonationDate = DateTime.UtcNow.AddMonths(-6),
                            Address          = "Nasr City, Cairo",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Pending,
                            CreatedAt        = DateTime.UtcNow.AddDays(-3),
                            ConfirmedAt      = null,
                        },
                        new Donation
                        {
                            DonorUserId      = D(1),
                            BloodRequestId   = null,
                            HospitalId       = H_Cleopatra,
                            BloodType        = BloodType.B_Negative,
                            Age = 24, Weight = 58, HasTattoo = false,
                            LastDonationDate = null,
                            Address          = "Mohandessin, Giza",
                            MedicalCondition = "False",   // no blocking medical condition
                            Status           = DonationStatus.Pending,
                            CreatedAt        = DateTime.UtcNow.AddDays(-1),
                            ConfirmedAt      = null,
                        },
                        new Donation
                        {
                            DonorUserId      = D(3),
                            BloodRequestId   = null,
                            HospitalId       = H_AlSalam,
                            BloodType        = BloodType.AB_Positive,
                            Age = 26, Weight = 62, HasTattoo = true,
                            LastDonationDate = DateTime.UtcNow.AddMonths(-7),
                            Address          = "Smouha, Alexandria",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Rejected,
                            CreatedAt        = DateTime.UtcNow.AddDays(-14),
                            ConfirmedAt      = null,
                        },
                        new Donation
                        {
                            DonorUserId      = D(5),
                            BloodRequestId   = null,
                            HospitalId       = H_AinShams,
                            BloodType        = BloodType.O_Negative,
                            Age = 22, Weight = 55, HasTattoo = false,
                            LastDonationDate = null,
                            Address          = "Cleopatra, Alexandria",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Pending,
                            CreatedAt        = DateTime.UtcNow.AddHours(-6),
                            ConfirmedAt      = null,
                        },
                        new Donation
                        {
                            DonorUserId      = D(7),
                            BloodRequestId   = null,
                            HospitalId       = H_ZayedSpecialized,
                            BloodType        = BloodType.A_Negative,
                            Age = 27, Weight = 65, HasTattoo = false,
                            LastDonationDate = DateTime.UtcNow.AddMonths(-9),
                            Address          = "Heliopolis, Cairo",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Cancelled,
                            CreatedAt        = DateTime.UtcNow.AddDays(-22),
                            ConfirmedAt      = null,
                        },
                        new Donation
                        {
                            DonorUserId      = D(9),
                            BloodRequestId   = null,
                            HospitalId       = H_Oct6University,
                            BloodType        = BloodType.A_Positive,
                            Age = 25, Weight = 60, HasTattoo = false,
                            LastDonationDate = DateTime.UtcNow.AddMonths(-3),
                            Address          = "Mansoura, Dakahlia",
                            MedicalCondition = "False",
                            Status           = DonationStatus.Pending,
                            CreatedAt        = DateTime.UtcNow.AddHours(-2),
                            ConfirmedAt      = null,
                        },
                    };

                    await context.Donations.AddRangeAsync(donations);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Seeded {donations.Count} donations.");
                }
                else
                {
                    Console.WriteLine("⏭  Donations already seeded.");
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
                // 6b. BLOOD BAGS  (one per Confirmed donation — feeds inventory)
                // ═════════════════════════════════════════════════════════════

                if (!await context.BloodBags.AnyAsync())
                {
                    var confirmedForBags = await context.Donations
                        .Where(d => d.Status == DonationStatus.Confirmed
                                 && d.HospitalId != null)
                        .ToListAsync();

                    var bloodBags = confirmedForBags.Select(d => new BloodBag
                    {
                        DonationId = d.Id,
                        HospitalId = d.HospitalId!.Value,
                        BloodType = d.BloodType,
                        Status = BloodBagStatus.Available,
                        CreatedAt = d.ConfirmedAt ?? DateTime.UtcNow,
                        // Standard blood-bank shelf life: 42 days from collection
                        ExpiryDate = (d.ConfirmedAt ?? DateTime.UtcNow).AddDays(42),
                    }).ToList();

                    if (bloodBags.Any())
                    {
                        await context.BloodBags.AddRangeAsync(bloodBags);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"✅ Seeded {bloodBags.Count} blood bags.");
                    }
                }
                else
                {
                    Console.WriteLine("⏭  BloodBags already seeded.");
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