using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BloodDonationSystem.Enums;

namespace BloodDonationSystem.Models
{
    public class BloodRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RequestedByUserId { get; set; }

        [ForeignKey(nameof(RequestedByUserId))]
        public ApplicationUser RequestedByUser { get; set; } = null!;

        
        public int? HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital? Hospital { get; set; } 

        [Required]
        [MaxLength(200)]
        public string HospitalName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string HospitalAddress { get; set; } = string.Empty;

        public BloodType BloodType { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

         public RequestPriority Priority { get; set; }

        public BloodRequestStatus Status { get; set; } = BloodRequestStatus.Open;

        public bool IsBloodReceived { get; set; } = false;

        public DateTime? neededby { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ─── Navigation properties ──────────────────────────────────────────────
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public PickupScan? PickupScan { get; set; }
        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
        public QrToken? QrToken { get; set; }
    }
}
