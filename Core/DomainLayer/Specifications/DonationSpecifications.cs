using BloodDonationSystem.Models;
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
}
