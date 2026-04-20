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

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<User> HospitalAdmins { get; set; } = new List<User>();
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<HospitalInventory> HospitalInventories { get; set; } = new List<HospitalInventory>();
        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
