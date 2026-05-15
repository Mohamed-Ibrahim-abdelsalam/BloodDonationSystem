using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    /// <summary>
    /// Fetch a single Hospital by Id — used to validate hospital exists before donation.
    /// </summary>
    public class HospitalByIdSpecification : BaseSpecification<Hospital>
    {
        public HospitalByIdSpecification(int id)
        {
            Criteria = h => h.Id == id;
        }
    }
}
