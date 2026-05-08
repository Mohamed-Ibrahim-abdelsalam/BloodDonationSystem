using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    // ══════════════════════════════════════════════════════════════════════════
    // BLOOD REQUESTS — COUNT Specifications (no includes, no ordering, no paging)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>COUNT — App Admin: total of ALL requests.</summary>
    public class AllRequestsCountSpecification : BaseSpecification<BloodRequest> { }

    /// <summary>COUNT — Hospital Admin: requests for a specific hospital.</summary>
    public class HospitalRequestsCountSpecification : BaseSpecification<BloodRequest>
    {
        public HospitalRequestsCountSpecification(int hospitalId)
            => Criteria = r => r.HospitalId == hospitalId;
    }

    /// <summary>COUNT — requests matching a status (App Admin scope).</summary>
    public class RequestsByStatusCountSpec : BaseSpecification<BloodRequest>
    {
        public RequestsByStatusCountSpec(BloodRequestStatus status)
            => Criteria = r => r.Status == status;
    }

    /// <summary>COUNT — requests matching a status for a specific hospital.</summary>
    public class RequestsByStatusAndHospitalCountSpec : BaseSpecification<BloodRequest>
    {
        public RequestsByStatusAndHospitalCountSpec(BloodRequestStatus status, int hospitalId)
            => Criteria = r => r.Status == status && r.HospitalId == hospitalId;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // BLOOD REQUESTS — PAGED DATA Specifications (includes + ordering + Skip/Take)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// PAGED DATA — App Admin: all requests with user included, ordered newest first.
    /// </summary>
    public class AllRequestsPagedSpecification : BaseSpecification<BloodRequest>
    {
        public AllRequestsPagedSpecification(int pageNumber, int pageSize)
        {
            AddInclude(r => r.RequestedByUser);
            ApplyOrderByDesc(r => r.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    /// <summary>
    /// PAGED DATA — Hospital Admin: requests for their hospital, ordered newest first.
    /// </summary>
    public class HospitalRequestsPagedSpecification : BaseSpecification<BloodRequest>
    {
        public HospitalRequestsPagedSpecification(int hospitalId, int pageNumber, int pageSize)
        {
            AddInclude(r => r.RequestedByUser);
            Criteria = r => r.HospitalId == hospitalId;
            ApplyOrderByDesc(r => r.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DONATIONS — COUNT Specifications
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>COUNT — App Admin: total of ALL donations.</summary>
    public class AllDonationsCountSpecification : BaseSpecification<Donation> { }

    /// <summary>COUNT — Hospital Admin: donations linked to their hospital.</summary>
    public class HospitalDonationsCountSpecification : BaseSpecification<Donation>
    {
        public HospitalDonationsCountSpecification(int hospitalId)
            => Criteria = d => d.HospitalId == hospitalId;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DONATIONS — STATS Specifications (includes BloodRequest for Quantity sum)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// STATS — App Admin: all donations with BloodRequest included for Quantity sum.
    /// No paging — fetches full filtered set to aggregate TotalQuantity.
    /// </summary>
    public class AllDonationsStatsSpecification : BaseSpecification<Donation>
    {
        public AllDonationsStatsSpecification()
            => AddInclude(d => d.BloodRequest);
    }

    /// <summary>
    /// STATS — Hospital Admin: hospital's donations with BloodRequest for Quantity sum.
    /// </summary>
    public class HospitalDonationsStatsSpecification : BaseSpecification<Donation>
    {
        public HospitalDonationsStatsSpecification(int hospitalId)
        {
            AddInclude(d => d.BloodRequest);
            Criteria = d => d.HospitalId == hospitalId;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DONATIONS — PAGED DATA Specifications
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// PAGED DATA — App Admin: all donations with user + request included, newest first.
    /// </summary>
    public class AllDonationsPagedSpecification : BaseSpecification<Donation>
    {
        public AllDonationsPagedSpecification(int pageNumber, int pageSize)
        {
            AddInclude(d => d.DonorUser);
            AddInclude(d => d.BloodRequest);
            ApplyOrderByDesc(d => d.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    /// <summary>
    /// PAGED DATA — Hospital Admin: hospital's donations with user + request, newest first.
    /// </summary>
    public class HospitalDonationsPagedSpecification : BaseSpecification<Donation>
    {
        public HospitalDonationsPagedSpecification(int hospitalId, int pageNumber, int pageSize)
        {
            AddInclude(d => d.DonorUser);
            AddInclude(d => d.BloodRequest);
            Criteria = d => d.HospitalId == hospitalId;
            ApplyOrderByDesc(d => d.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
