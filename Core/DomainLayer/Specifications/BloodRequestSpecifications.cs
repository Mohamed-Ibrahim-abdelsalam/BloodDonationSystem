using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{

    /// <summary>\n'
    /// Fetches OPEN requests for AI matching — excludes the current user\'s own requests\n'
    /// so donors cannot be matched to their own blood request.\n'
    /// </summary>\n'
    public class OpenBloodRequestsForAiSpecification : BaseSpecification<BloodRequest>
    {
        public OpenBloodRequestsForAiSpecification(
            string excludeUserId,
            BloodType? bloodType = null,
            RequestPriority? priority = null,
           string? search = null)
        {
            AddInclude(r => r.RequestedByUser);
                AddInclude(r => r.Hospital);


                var searchLower = search?.ToLower();


                Criteria = r =>
                    r.Status == BloodRequestStatus.Open &&
                    r.RequestedByUserId != excludeUserId &&   // never show own requests\n'
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
                AddInclude(r => r.Hospital);
    
                Criteria = r => r.Id == id;
    
                // Tracked — loaded for mutation (Status, IsBloodReceived, etc.)\n'
                DisableReadOnly();
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
