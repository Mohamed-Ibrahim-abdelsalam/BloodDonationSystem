using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{

    /// <summary>
    /// GET /api/ai/match-requests — fetches OPEN requests for AI matching.
    /// Filters by BloodType, Priority, and case-insensitive search on HospitalName or HospitalLocation.
    /// Includes Hospital nav-prop for HospitalName fallback.
    /// </summary>
    public class OpenBloodRequestsForAiSpecification : BaseSpecification<BloodRequest>
    {
        public OpenBloodRequestsForAiSpecification(
            BloodType? bloodType = null,
            RequestPriority? priority = null,
            string? search = null)
        {
            AddInclude(r => r.RequestedByUser);
            AddInclude(r => r.Hospital);

            var searchLower = search?.ToLower();

            Criteria = r =>
                r.Status == BloodRequestStatus.Open &&
                (!bloodType.HasValue || r.BloodType == bloodType.Value) &&
                (!priority.HasValue || r.Priority == priority.Value) &&
                (string.IsNullOrEmpty(searchLower) ||
                    r.HospitalName.ToLower().Contains(searchLower) ||
                    r.HospitalLocation.ToLower().Contains(searchLower));
        }
    }

    /// <summary>
    /// GET /api/requests/{id} — fetches single request with full details
    /// </summary>
    public class BloodRequestByIdSpecification : BaseSpecification<BloodRequest>
    {
        public BloodRequestByIdSpecification(int id)
        { 
    
            AddInclude(r => r.RequestedByUser);
            AddInclude(r => r.Hospital);   // needed for HospitalName in response
    
            Criteria = r => r.Id == id;
        }
    }

    /// <summary>
    /// GET /api/requests/my — fetches all requests for a specific user
    /// </summary>
    public class BloodRequestByUserSpecification : BaseSpecification<BloodRequest>
    {
        public BloodRequestByUserSpecification(string userId)
        {
            AddInclude(r => r.RequestedByUser);
            AddInclude(r => r.Hospital);   // needed for HospitalName in response

            Criteria = r => r.RequestedByUserId == userId;

            // Newest first
            ApplyOrderByDesc(r => r.CreatedAt);
        }
    }
}
