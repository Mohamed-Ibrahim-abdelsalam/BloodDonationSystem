using System.ComponentModel.DataAnnotations;

namespace BloodDonationSystem.Models
{
    public class Hospital
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<ApplicationUser> HospitalAdmins { get; set; } = new List<ApplicationUser>();
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<HospitalInventory> HospitalInventories { get; set; } = new List<HospitalInventory>();
        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
