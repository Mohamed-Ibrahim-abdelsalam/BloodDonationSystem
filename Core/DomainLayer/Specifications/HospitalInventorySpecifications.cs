using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    /// <summary>
    /// Fetches all HospitalInventory records for a given hospital,
    /// ordered by BloodType ASC for consistent display.
    /// Includes Hospital nav-prop for the hospital name in the response.
    /// </summary>
    public class HospitalInventoryByHospitalSpecification : BaseSpecification<HospitalInventory>
    {
        public HospitalInventoryByHospitalSpecification(int hospitalId)
        {
            Criteria = inv => inv.HospitalId == hospitalId;
            AddInclude(inv => inv.Hospital);
            ApplyOrderBy(inv => inv.BloodType);
        }
    }
}
