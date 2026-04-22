using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BloodDonationSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

       
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<DonationScan> DonationScans { get; set; }
        public DbSet<PickupScan> PickupScans { get; set; }
        public DbSet<QrToken> QrTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<UserReward> UserRewards { get; set; }
        public DbSet<HospitalInventory> HospitalInventories { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ApplicationUser configuration///////

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.NationalId).IsUnique();
                entity.Property(u => u.Gender).HasConversion<string>();
                entity.Property(u => u.BloodType).HasConversion<string>();
                entity.Property(u => u.Points).HasDefaultValue(0);

                entity.HasOne(u => u.Hospital)
                      .WithMany(h => h.HospitalAdmins)
                      .HasForeignKey(u => u.HospitalId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // RefreshToken configuration//////////

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(r => r.Token).IsUnique();
                entity.HasOne(r => r.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Index on Hospital Name for faster search////////////

            modelBuilder.Entity<Hospital>(entity =>
            {
                entity.HasIndex(h => h.Name);
            });

            // BloodRequest configuration/////////////////

            modelBuilder.Entity<BloodRequest>(entity =>
            {
                entity.Property(br => br.Status).HasConversion<string>();
                entity.Property(br => br.Priority).HasConversion<string>();
                entity.Property(br => br.BloodType).HasConversion<string>();
                entity.Property(br => br.IsBloodReceived).HasDefaultValue(false);

                entity.HasOne(br => br.RequestedByUser)
                      .WithMany(u => u.BloodRequests)
                      .HasForeignKey(br => br.RequestedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);


                entity.HasOne(br => br.Hospital)
                     .WithMany(h => h.BloodRequests)
                     .HasForeignKey(br => br.HospitalId)
                     .IsRequired(false)
                     .OnDelete(DeleteBehavior.SetNull);

            });


            // Donation configuration/////////////////

            modelBuilder.Entity<Donation>(entity =>
            {
                entity.Property(d => d.Status).HasConversion<string>();
                entity.Property(d => d.BloodType).HasConversion<string>();

                entity.HasOne(d => d.DonorUser)
                      .WithMany(u => u.Donations)
                      .HasForeignKey(d => d.DonorUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.BloodRequest)
                      .WithMany(br => br.Donations)
                      .HasForeignKey(d => d.BloodRequestId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Hospital)
                      .WithMany(h => h.Donations)
                      .HasForeignKey(d => d.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // DonationScan configuration/////////////////

            modelBuilder.Entity<DonationScan>(entity =>
            {
                entity.HasIndex(s => s.DonationId).IsUnique();
                entity.HasOne(s => s.Donation)
                      .WithOne(d => d.DonationScan)
                      .HasForeignKey<DonationScan>(s => s.DonationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.ScannedByHospitalAdmin)
                      .WithMany(u => u.DonationScans)
                      .HasForeignKey(s => s.ScannedByHospitalAdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // PickupScan configuration/////////////////

            modelBuilder.Entity<PickupScan>(entity =>
            {
                entity.HasIndex(p => p.BloodRequestId).IsUnique();
                entity.HasOne(p => p.BloodRequest)
                      .WithOne(br => br.PickupScan)
                      .HasForeignKey<PickupScan>(p => p.BloodRequestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ScannedByUser)
                      .WithMany(u => u.PickupScans)
                      .HasForeignKey(p => p.ScannedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });



            // QrToken configuration/////////////////

            modelBuilder.Entity<QrToken>(entity =>
            {
                entity.HasIndex(q => q.Token).IsUnique();
                entity.Property(q => q.Type).HasConversion<string>();
                entity.Property(q => q.IsUsed).HasDefaultValue(false);
            });



            // Notification configuration/////////////////

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.IsRead).HasDefaultValue(false);
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });



            // Reward configuration/////////////////

            modelBuilder.Entity<UserReward>(entity =>
            {
                entity.HasIndex(ur => new { ur.UserId, ur.RewardId }).IsUnique();
                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRewards)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Reward)
                      .WithMany(r => r.UserRewards)
                      .HasForeignKey(ur => ur.RewardId)
                      .OnDelete(DeleteBehavior.Cascade);
            });



            // HospitalInventory configuration/////////////////

            modelBuilder.Entity<HospitalInventory>(entity =>
            {
                entity.HasIndex(hi => new { hi.HospitalId, hi.BloodType }).IsUnique();
                entity.Ignore(hi => hi.Status);

                entity.HasOne(hi => hi.Hospital)
                      .WithMany(h => h.HospitalInventories)
                      .HasForeignKey(hi => hi.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(hi => hi.UpdatedByAdmin)
                      .WithMany(u => u.UpdatedInventories)
                      .HasForeignKey(hi => hi.UpdatedByAdminId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });




            // InventoryLog configuration/////////////////


            modelBuilder.Entity<InventoryLog>(entity =>
            {
                entity.HasOne(il => il.HospitalInventory)
                      .WithMany(hi => hi.InventoryLogs)
                      .HasForeignKey(il => il.HospitalInventoryId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(il => il.Hospital)
                      .WithMany(h => h.InventoryLogs)
                      .HasForeignKey(il => il.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(il => il.Donation)
                      .WithMany(d => d.InventoryLogs)
                      .HasForeignKey(il => il.DonationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(il => il.BloodRequest)
                      .WithMany(br => br.InventoryLogs)
                      .HasForeignKey(il => il.BloodRequestId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(il => il.ChangedByAdmin)
                      .WithMany(u => u.InventoryLogs)
                      .HasForeignKey(il => il.ChangedByAdminId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
