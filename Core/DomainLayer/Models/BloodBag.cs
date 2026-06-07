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
    public class BloodBag
    {
        [Key]
        public int Id { get; set; }

    
        [Required]
        public int DonationId { get; set; }

        [ForeignKey(nameof(DonationId))]
        public Donation Donation { get; set; } = null!;

        [Required]
        public int HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital Hospital { get; set; } = null!;

     
        [Required]
        public BloodType BloodType { get; set; }

        public BloodBagStatus Status { get; set; } = BloodBagStatus.Available;

        public DateTime? WithdrawnAt { get; set; }

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
