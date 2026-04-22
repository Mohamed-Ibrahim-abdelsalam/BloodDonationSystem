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
    /// GET /api/requests — supports filtering by BloodType, Priority, and search by creator FullName
    /// Orders: Emergency first, then newest
    /// </summary>
    public class BloodRequestSpecification : BaseSpecification<BloodRequest>
    {
        public BloodRequestSpecification(
            BloodType? bloodType = null,
            RequestPriority? priority = null,
            string? search = null)
        {
            // Always include the creator user for FullName
            AddInclude(r => r.RequestedByUser);

            // Build criteria chain
            Criteria = r =>
                (!bloodType.HasValue || r.BloodType == bloodType.Value) &&
                (!priority.HasValue || r.Priority == priority.Value) &&
                (string.IsNullOrEmpty(search) ||
                    r.RequestedByUser.FullName.ToLower().Contains(search.ToLower()));

            // Emergency first → then newest
            ApplyOrderByDesc(r => r.Priority);
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

            Criteria = r => r.RequestedByUserId == userId;

            // Newest first
            ApplyOrderByDesc(r => r.CreatedAt);
        }
    }
}
