using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    public class InventoryDashboardDto
    {
        public int TotalUnits { get; set; }
        public int High { get; set; }
        public int Low { get; set; }
        public int Critical { get; set; }
    }

    /// <summary>
    /// Single blood-type row in the inventory list.
    /// Status is calculated dynamically — not stored in DB.
    /// nearestExpiryDate surfaces the oldest bag (FIFO).
    /// </summary>
    public class InventoryItemDto
    {
        public string BloodType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime? NearestExpiryDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Full response for GET /api/hospital/inventory.
    public class HospitalInventoryResponseDto
    {
        public int HospitalId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public InventoryDashboardDto Dashboard { get; set; } = new();
        public IEnumerable<InventoryItemDto> Inventory { get; set; } = new List<InventoryItemDto>();
    }
}
