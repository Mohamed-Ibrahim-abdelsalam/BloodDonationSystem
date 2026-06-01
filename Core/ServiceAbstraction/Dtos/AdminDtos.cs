using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{

    // ══════════════════════════════════════════════════════════════════════════
    // BLOOD REQUESTS Dashboard DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Aggregated statistics calculated from the FULL filtered set (before pagination).
    /// All counts respect the caller's role-based filter.
    /// </summary>
    public class RequestStatisticsDto
    {
        public int TotalRequests { get; set; }
        public int OpenRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public int CompletedRequests { get; set; }
    }

    /// <summary>
    /// Single request row displayed in the admin dashboard list.
    /// </summary>
    public class AdminRequestItemDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Full response for GET /api/admin/requests.
    /// Statistics cover the entire filtered dataset;
    /// Requests is the paginated current page only.
    /// </summary>
    public class AdminRequestsDashboardDto
    {
        public RequestStatisticsDto Statistics { get; set; } = new();

        /// <summary>
        /// Paginated page of request items — wraps CurrentPage, PageSize,
        /// TotalCount, TotalPages, HasPrevious, HasNext, and Data.
        /// </summary>
        public PaginatedResponse<AdminRequestItemDto> Requests { get; set; } = null!;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DONATIONS Dashboard DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Aggregated statistics calculated from the FULL filtered set (before pagination).
    /// </summary>
    public class DonationStatisticsDto
    {
        public int TotalDonations { get; set; }

        /// <summary>
        /// Sum of Quantity from linked BloodRequests.
        /// Falls back to 1 per donation when no BloodRequest is linked (general donation).
        /// </summary>
        public int TotalQuantity { get; set; }
    }

    /// <summary>
    /// Single donation row displayed in the admin dashboard list.
    /// </summary>
    public class AdminDonationItemDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;

        public int? BloodRequestId { get; set; }

        /// <summary>
        /// Quantity from the linked BloodRequest, or 1 for general donations.
        /// </summary>
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Full response for GET /api/admin/donations.
    /// Statistics cover the entire filtered dataset;
    /// Donations is the paginated current page only.
    /// </summary>
    public class AdminDonationsDashboardDto
    {
        public DonationStatisticsDto Statistics { get; set; } = new();

        /// <summary>
        /// Paginated page of donation items.
        /// </summary>
        public PaginatedResponse<AdminDonationItemDto> Donations { get; set; } = null!;
    }


    // ══════════════════════════════════════════════════════════════════════════
    // USERS Management DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Single row in the paginated users list.
    /// Status is computed from LastDonationDate: Active (≤ 3 months), Inactive otherwise.
    /// </summary>
    public class AdminUserListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public int Points { get; set; }

        /// <summary>Date of the most recent confirmed donation. Null if never donated.</summary>
        public DateTime? LastDonation { get; set; }

        /// <summary>"Active" if donated within last 3 months, otherwise "Inactive".</summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Full user profile for GET /api/admin/users/{id} — App Admin only.
    /// </summary>
    public class AdminUserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public int Points { get; set; }

        /// <summary>Date of the most recent confirmed donation. Null if never donated.</summary>
        public DateTime? LastDonation { get; set; }

        /// <summary>"Active" or "Inactive" based on LastDonation within 3 months.</summary>
        public string Status { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        /// <summary>Count of all donations regardless of status.</summary>
        public int TotalDonations { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
