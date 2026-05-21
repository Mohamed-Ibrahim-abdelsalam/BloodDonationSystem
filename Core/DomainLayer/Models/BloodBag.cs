using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    /// <summary>
    /// Represents a single physical blood bag created automatically
    /// when a donation QR is successfully scanned and confirmed.
    /// Each donation creates exactly one BloodBag.
    /// </summary>
    public class BloodBag
    {
        [Key]
        public int Id { get; set; }

        /// <summary>The confirmed donation that produced this blood bag.</summary>
        [Required]
        public int DonationId { get; set; }

        [ForeignKey(nameof(DonationId))]
        public Donation Donation { get; set; } = null!;

        /// <summary>Hospital where the bag is stored.</summary>
        [Required]
        public int HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital Hospital { get; set; } = null!;

        /// <summary>Blood type inherited from the donation at creation time.</summary>
        [Required]
        public BloodType BloodType { get; set; }

        public BloodBagStatus Status { get; set; } = BloodBagStatus.Available;

        /// <summary>
        /// Timestamp of the successful donation QR scan — when the bag entered inventory.
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Auto-calculated: CreatedAt + 42 days.
        [Required]
        public DateTime ExpiryDate { get; set; }
    }
}
