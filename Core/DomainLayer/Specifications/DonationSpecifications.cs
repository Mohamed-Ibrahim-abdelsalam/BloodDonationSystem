using BloodDonationSystem.Models;
using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    /// <summary>
    /// GET /api/donations/my — all donations for current user, newest first
    /// Includes BloodRequest (for HospitalName) and Hospital
    /// </summary>
    public class DonationsByUserSpecification : BaseSpecification<Donation>
    {
        public DonationsByUserSpecification(string userId)
        {
            AddInclude(d => d.BloodRequest);
            AddInclude(d => d.Hospital);

            Criteria = d => d.DonorUserId == userId;

            ApplyOrderByDesc(d => d.CreatedAt);
        }
    }

    /// <summary>
    /// GET single donation by id — with full relations
    /// </summary>
    public class DonationByIdSpecification : BaseSpecification<Donation>
    {
        public DonationByIdSpecification(int id)
        {
            AddInclude(d => d.BloodRequest);
            AddInclude(d => d.Hospital);
            AddInclude(d => d.DonorUser);

            Criteria = d => d.Id == id;
        }
    }

    /// <summary>
    /// Check for duplicate donation (same user + same request)
    /// </summary>
    public class DuplicateDonationSpecification : BaseSpecification<Donation>
    {
        public DuplicateDonationSpecification(string userId, int bloodRequestId)
        {
            Criteria = d =>
                d.DonorUserId == userId &&
                d.BloodRequestId == bloodRequestId;
        }
    }


    // ── Fix 2: Blood Bag specs ─────────────────────────────────────────────────

    /// <summary>
    /// Fetch the BloodBag linked to a specific donation.
    /// Used to mark bag as Withdrawn when pickup scan Case 2 occurs.
    /// </summary>
    public class BloodBagByDonationSpecification : BaseSpecification<BloodBag>
    {
        public BloodBagByDonationSpecification(int donationId)
        {
            Criteria = bag => bag.DonationId == donationId;
        }
    }

    // ── Fix 1: Count confirmed donations for a request ─────────────────────────

    /// <summary>
    /// COUNT — confirmed donations linked to a specific blood request.
    /// Used to check if confirmed count >= request.Quantity before Fulfilling.
    /// </summary>
    public class ConfirmedDonationsByRequestSpecification : BaseSpecification<Donation>
    {
        public ConfirmedDonationsByRequestSpecification(int bloodRequestId)
        {
            Criteria = d =>
                d.BloodRequestId == bloodRequestId &&
                d.Status == BloodDonationSystem.Enums.DonationStatus.Confirmed;
        }
    }

}
