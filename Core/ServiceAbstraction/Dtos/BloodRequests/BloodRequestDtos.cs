using BloodDonationSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos.BloodRequests
{
    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class CreateBloodRequestDto
    {
        public BloodType BloodType { get; set; }
        public int Quantity { get; set; }
        public RequestPriority Priority { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalAddress { get; set; } = string.Empty;
        public DateTime? NeededBy { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────

    /// <summary>
    /// Used in POST response and GET /api/requests list
    /// </summary>
    public class BloodRequestDto
    {
        public int Id { get; set; }
        public BloodType BloodType { get; set; }
        public int Quantity { get; set; }
        public RequestPriority Priority { get; set; }
        public BloodRequestStatus Status { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalAddress { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;   // FullName
        public DateTime CreatedAt { get; set; }
        public DateTime? NeededBy { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Used in GET /api/requests/{id} — full details
    /// </summary>
    public class BloodRequestDetailDto
    {
        public int Id { get; set; }
        public BloodType BloodType { get; set; }
        public int Quantity { get; set; }
        public RequestPriority Priority { get; set; }
        public BloodRequestStatus Status { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalAddress { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? NeededBy { get; set; }
    }

    /// <summary>
    /// Used in GET /api/requests/my
    /// </summary>
    public class MyBloodRequestDto
    {
        public int Id { get; set; }
        public BloodType BloodType { get; set; }
        public int Quantity { get; set; }
        public RequestPriority Priority { get; set; }
        public BloodRequestStatus Status { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? NeededBy { get; set; }
    }

    // ── Query Params ──────────────────────────────────────────────────────────

    public class BloodRequestQueryParams
    {
        public BloodType? BloodType { get; set; }
        public RequestPriority? Priority { get; set; }
        public string? Search { get; set; }  // search by creator FullName
    }
}
