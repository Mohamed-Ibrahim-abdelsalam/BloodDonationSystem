using DomainLayer.Enums;
using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
 
    public class AvailableBloodBagsByHospitalSpecification : BaseSpecification<BloodBag>
    {
        public AvailableBloodBagsByHospitalSpecification(int hospitalId)
        {
            Criteria = bag =>
                bag.HospitalId == hospitalId &&
                bag.Status == BloodBagStatus.Available;

            AddInclude(bag => bag.Hospital);

            // Order by ExpiryDate ASC — surfaces nearest-to-expire bags first (FIFO)
            ApplyOrderBy(bag => bag.ExpiryDate);
        }
    }


    /// <summary>
    /// Fetches ALL blood bags for a hospital — both Available and Withdrawn.
    /// Used by the prediction service which needs full historical data for ML.
    /// </summary>
    public class AllBloodBagsByHospitalSpecification : BaseSpecification<BloodBag>
    {
        public AllBloodBagsByHospitalSpecification(int hospitalId)
        {
            Criteria = bag => bag.HospitalId == hospitalId;
            ApplyOrderBy(bag => bag.CreatedAt);
        }
    }


    /// <summary>
    /// Fetches blood bags that were WITHDRAWN within a given date range for a hospital.
    /// Used by the Blood Usage Analytics endpoint.
    /// WithdrawnAt falls back to ExpiryDate.AddDays(-42) for historical bags
    /// that predate the WithdrawnAt field.
    /// </summary>
    public class WithdrawnBloodBagsByPeriodSpecification : BaseSpecification<BloodBag>
    {
        public WithdrawnBloodBagsByPeriodSpecification(int hospitalId, DateTime dateFrom)
        {
            Criteria = bag =>
                bag.HospitalId == hospitalId &&
                bag.Status == BloodBagStatus.Withdrawn &&
                (bag.WithdrawnAt.HasValue
                    ? bag.WithdrawnAt.Value >= dateFrom
                    : bag.CreatedAt >= dateFrom);   // fallback for legacy data

            ApplyOrderByDesc(bag => bag.WithdrawnAt ?? bag.CreatedAt);
        }
    }
}
