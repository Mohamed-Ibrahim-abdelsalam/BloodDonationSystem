using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{

    /// <summary>Fetch a single Hospital by Id.</summary>
    public class HospitalByIdSpecification : BaseSpecification<Hospital>
    {
        public HospitalByIdSpecification(int id)
        {
            Criteria = h => h.Id == id;
        }
    }

    /// <summary>
    /// GET /api/admin/hospitals — all hospitals ordered by Name ASC.
    /// </summary>
    public class AllHospitalsSpecification : BaseSpecification<Hospital>
    {
        public AllHospitalsSpecification()
        {
            ApplyOrderBy(h => h.Name);
        }
    }

    /// <summary>
    /// Duplicate-name check — finds any hospital with the given name
    /// excluding a specific id (used during update to ignore self).
    /// </summary>
    public class HospitalByNameSpecification : BaseSpecification<Hospital>
    {
        public HospitalByNameSpecification(string name, int excludeId = 0)
        {
            Criteria = h =>
                h.Name.ToLower() == name.ToLower() &&
                h.Id != excludeId;
        }
    }

    /// <summary>
    /// Duplicate-email check — finds any hospital with the given email
    /// excluding a specific id (used during update to ignore self).
    /// </summary>
    public class HospitalByEmailSpecification : BaseSpecification<Hospital>
    {
        public HospitalByEmailSpecification(string email, int excludeId = 0)
        {
            Criteria = h =>
                h.Email != null &&
                h.Email.ToLower() == email.ToLower() &&
                h.Id != excludeId;
        }
    }

    /// <summary>
    /// COUNT — total number of hospitals in the system.
    /// </summary>
    public class AllHospitalsCountSpecification : BaseSpecification<Hospital> { }

    /// <summary>
    /// GET /api/hospitals/dropdown — lightweight list ordered by Name ASC.
    /// No nav-prop includes needed; only Id and Name are projected in the service.
    /// </summary>
    public class HospitalsDropdownSpecification : BaseSpecification<Hospital>
    {
        public HospitalsDropdownSpecification()
        {
            ApplyOrderBy(h => h.Name);
        }
    }
}
