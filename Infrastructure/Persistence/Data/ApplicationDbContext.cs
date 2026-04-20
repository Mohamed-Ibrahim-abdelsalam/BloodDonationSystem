using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ─── DbSets ────────────────────────────────────────────────────────────
        public DbSet<User>                      Users                      { get; set; }
        public DbSet<Hospital>                   Hospitals                  { get; set; }
        public DbSet<BloodRequest>               BloodRequests              { get; set; }
        public DbSet<Donation>                   Donations                  { get; set; }
        public DbSet<DonationScan>               DonationScans              { get; set; }
        public DbSet<PickupScan>                 PickupScans                { get; set; }
        public DbSet<QrToken>                    QrTokens                   { get; set; }
        public DbSet<Notification>               Notifications              { get; set; }
        public DbSet<Reward>                     Rewards                    { get; set; }
        public DbSet<UserReward>                 UserRewards                { get; set; }
        public DbSet<HospitalInventory>          HospitalInventories        { get; set; }
        public DbSet<InventoryLog>               InventoryLogs              { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ═══════════════════════════════════════════════════════════════════
            // User
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.Role)
                      .HasConversion<string>();

                entity.Property(u => u.Points)
                      .HasDefaultValue(0);

                 entity.Property(u => u.Gender)
                      .HasConversion<string>();

                entity.HasIndex(u => u.NationalId)
                      .IsUnique();

                entity.Property(u => u.NationalId)
                      .IsRequired();

                entity.Property(u => u.Address)
                      .IsRequired();
;
       
                entity.Property(b => b.BloodType)
                      .HasConversion<string>(); 
       
                // One Hospital has many HospitalAdmins (Users)
                entity.HasOne(u => u.Hospital)
                      .WithMany(h => h.HospitalAdmins)
                      .HasForeignKey(u => u.HospitalId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ═══════════════════════════════════════════════════════════════════
            // Hospital
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<Hospital>(entity =>
            {
                entity.HasIndex(h => h.Name);
            });

            // ═══════════════════════════════════════════════════════════════════
            // BloodRequest
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<BloodRequest>(entity =>
            {
                entity.Property(br => br.Status)
                      .HasConversion<string>();

                entity.Property(br => br.IsBloodReceived)
                      .HasDefaultValue(false);

                // Many BloodRequests → one User (requester)
                entity.HasOne(br => br.RequestedByUser)
                      .WithMany(u => u.BloodRequests)
                      .HasForeignKey(br => br.RequestedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many BloodRequests → one Hospital
                entity.HasOne(br => br.Hospital)
                      .WithMany(h => h.BloodRequests)
                      .HasForeignKey(br => br.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);

                 
                 entity.Property(r => r.Priority)
                       .HasConversion<string>();

                  entity.Property(b => b.BloodType)
                        .HasConversion<string>();    
      
            });

            // ═══════════════════════════════════════════════════════════════════
            // Donation
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.Property(d => d.Status)
                      .HasConversion<string>();

                entity.Property(b => b.BloodType)
                      .HasConversion<string>();       

                // Many Donations → one User (donor)
                entity.HasOne(d => d.DonorUser)
                      .WithMany(u => u.Donations)
                      .HasForeignKey(d => d.DonorUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many Donations → one BloodRequest
                entity.HasOne(d => d.BloodRequest)
                      .WithMany(br => br.Donations)
                      .HasForeignKey(d => d.BloodRequestId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many Donations → one Hospital
                entity.HasOne(d => d.Hospital)
                      .WithMany(h => h.Donations)
                      .HasForeignKey(d => d.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // ═══════════════════════════════════════════════════════════════════
            // DonationScan  (one-to-one with Donation)
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<DonationScan>(entity =>
            {
                entity.HasIndex(s => s.DonationId)
                      .IsUnique();

                entity.HasOne(s => s.Donation)
                      .WithOne(d => d.DonationScan)
                      .HasForeignKey<DonationScan>(s => s.DonationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.ScannedByHospitalAdmin)
                      .WithMany(u => u.DonationScans)
                      .HasForeignKey(s => s.ScannedByHospitalAdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══════════════════════════════════════════════════════════════════
            // PickupScan  (one-to-one with BloodRequest)
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<PickupScan>(entity =>
            {
                entity.HasIndex(p => p.BloodRequestId)
                      .IsUnique();

                entity.HasOne(p => p.BloodRequest)
                      .WithOne(br => br.PickupScan)
                      .HasForeignKey<PickupScan>(p => p.BloodRequestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ScannedByUser)
                      .WithMany(u => u.PickupScans)
                      .HasForeignKey(p => p.ScannedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══════════════════════════════════════════════════════════════════
            // QrToken
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<QrToken>(entity =>
            {
                entity.HasIndex(q => q.Token)
                      .IsUnique();

                entity.Property(q => q.Type)
                      .HasConversion<string>();

                entity.Property(q => q.IsUsed)
                      .HasDefaultValue(false);
            });

      

            // ═══════════════════════════════════════════════════════════════════
            // Notification
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.IsRead)
                      .HasDefaultValue(false);

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ═══════════════════════════════════════════════════════════════════
            // Reward  (no extra Fluent API needed)
            // ═══════════════════════════════════════════════════════════════════

            // ═══════════════════════════════════════════════════════════════════
            // UserReward  (Many-to-Many join)
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<UserReward>(entity =>
            {
                entity.HasIndex(ur => new { ur.UserId, ur.RewardId })
                      .IsUnique();

                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRewards)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Reward)
                      .WithMany(r => r.UserRewards)
                      .HasForeignKey(ur => ur.RewardId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // ═══════════════════════════════════════════════════════════════════
            // HospitalInventory
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<HospitalInventory>(entity =>
            {
                entity.HasIndex(hi => new { hi.HospitalId, hi.BloodType })
                      .IsUnique();

                entity.Property(hi => hi.Status)
                      .HasConversion<string>();

                // Many HospitalInventories → one Hospital
                entity.HasOne(hi => hi.Hospital)
                      .WithMany(h => h.HospitalInventories)
                      .HasForeignKey(hi => hi.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many HospitalInventories → one User (admin, nullable)
                entity.HasOne(hi => hi.UpdatedByAdmin)
                      .WithMany(u => u.UpdatedInventories)
                      .HasForeignKey(hi => hi.UpdatedByAdminId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ═══════════════════════════════════════════════════════════════════
            // InventoryLog
            // ═══════════════════════════════════════════════════════════════════
            modelBuilder.Entity<InventoryLog>(entity =>
            {
                // Many InventoryLogs → one HospitalInventory
                entity.HasOne(il => il.HospitalInventory)
                      .WithMany(hi => hi.InventoryLogs)
                      .HasForeignKey(il => il.HospitalInventoryId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many InventoryLogs → one Hospital
                entity.HasOne(il => il.Hospital)
                      .WithMany(h => h.InventoryLogs)
                      .HasForeignKey(il => il.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many InventoryLogs → one Donation (nullable)
                entity.HasOne(il => il.Donation)
                      .WithMany(d => d.InventoryLogs)
                      .HasForeignKey(il => il.DonationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                // Many InventoryLogs → one BloodRequest (nullable)
                entity.HasOne(il => il.BloodRequest)
                      .WithMany(br => br.InventoryLogs)
                      .HasForeignKey(il => il.BloodRequestId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                // Many InventoryLogs → one User (admin, nullable)
                entity.HasOne(il => il.ChangedByAdmin)
                      .WithMany(u => u.InventoryLogs)
                      .HasForeignKey(il => il.ChangedByAdminId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
