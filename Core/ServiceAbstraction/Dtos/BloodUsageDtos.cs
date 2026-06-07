using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    /// <summary>Single blood-type usage row in the analytics response.</summary>
    public class BloodUsageItemDto
    {
        public string BloodType { get; set; } = string.Empty;
        public int UsedUnits { get; set; }

        /// <summary>Percentage of total used units. Rounded to 1 decimal place.</summary>
        public double Percentage { get; set; }
    }

    /// <summary>Full response for GET /api/hospital/blood-usage.</summary>
    public class BloodUsageResponseDto
    {
        public int HospitalId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public int TotalUsedUnits { get; set; }

        /// <summary>Per-blood-type breakdown, sorted by UsedUnits DESC.</summary>
        public IEnumerable<BloodUsageItemDto> BloodUsage { get; set; }
            = new List<BloodUsageItemDto>();
    }
}
