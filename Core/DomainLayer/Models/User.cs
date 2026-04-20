using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BloodDonationSystem.Enums;

namespace BloodDonationSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        public string? PhoneNumber { get; set; }

        public BloodType BloodType { get; set; }


        public Gender Gender { get; set; }

        public string NationalId { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        public int Points { get; set; } = 0;

        public Role Role { get; set; } = Role.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK: optional link to a Hospital (for HospitalAdmin role)
        public int? HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital? Hospital { get; set; }

        // ─── Navigation properties ──────────────────────────────────────────────
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<UserReward> UserRewards { get; set; } = new List<UserReward>();
        public ICollection<DonationScan> DonationScans { get; set; } = new List<DonationScan>();
        public ICollection<PickupScan> PickupScans { get; set; } = new List<PickupScan>();
        public ICollection<HospitalInventory> UpdatedInventories { get; set; } = new List<HospitalInventory>();
        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
