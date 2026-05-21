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
}
