using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    // ══════════════════════════════════════════════════════════════════════════
    // USER Specifications — Admin management endpoints
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// COUNT — all users whose Role == User (excludes admins).
    /// Used by App Admin to get TotalCount for pagination.
    /// </summary>
    public class AllUsersCountSpecification : BaseSpecification<ApplicationUser>
    {
        public AllUsersCountSpecification()
        {
            Criteria = u => u.Role == BloodDonationSystem.Enums.Role.User;
        }
    }

    /// <summary>
    /// PAGED DATA — App Admin: all regular users ordered by
    /// LastDonationDate DESC (nulls last via ConfirmedAt on latest donation —
    /// ordering is handled in-service after fetch for the null case).
    /// Includes Donations so we can resolve LastDonationDate.
    /// </summary>
    public class AllUsersPagedSpecification : BaseSpecification<ApplicationUser>
    {
        public AllUsersPagedSpecification(int pageNumber, int pageSize)
        {
            Criteria = u => u.Role == BloodDonationSystem.Enums.Role.User;
            AddInclude(u => u.Donations);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    /// <summary>
    /// All regular users with Donations included — NO paging.
    /// Used by App Admin to build the ordered count query in-memory
    /// when ordering by a computed value (LastDonationDate from Donations).
    /// </summary>
    public class AllUsersWithDonationsSpecification : BaseSpecification<ApplicationUser>
    {
        public AllUsersWithDonationsSpecification()
        {
            Criteria = u => u.Role == BloodDonationSystem.Enums.Role.User;
            AddInclude(u => u.Donations);
        }
    }

    /// <summary>
    /// GET /api/admin/users/{id} — single user with Donations for TotalDonations count.
    /// App Admin only.
    /// </summary>
    public class UserByIdWithDonationsSpecification : BaseSpecification<ApplicationUser>
    {
        public UserByIdWithDonationsSpecification(string userId)
        {
            Criteria = u => u.Id == userId;
            AddInclude(u => u.Donations);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DONATION Specifications — used to resolve hospital donor IDs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Fetches all confirmed donations for a given hospital.
    /// Used by Hospital Admin to discover which users donated at their hospital.
    /// Only confirmed donations qualify (Status == Confirmed).
    /// </summary>
    public class HospitalConfirmedDonationsSpecification : BaseSpecification<Donation>
    {
        public HospitalConfirmedDonationsSpecification(int hospitalId)
        {
            Criteria = d =>
                d.HospitalId == hospitalId &&
                d.Status == BloodDonationSystem.Enums.DonationStatus.Confirmed;
        }
    }

    /// <summary>
    /// Fetches confirmed donations for specific donor IDs at a hospital.
    /// Includes DonorUser so we can project user profile data.
    /// Used for the paginated user list in Hospital Admin scope.
    /// </summary>
    public class HospitalDonorDonationsSpecification : BaseSpecification<Donation>
    {
        public HospitalDonorDonationsSpecification(int hospitalId, IEnumerable<string> donorIds)
        {
            var donorIdSet = donorIds.ToHashSet();
            Criteria = d =>
                d.HospitalId == hospitalId &&
                d.Status == BloodDonationSystem.Enums.DonationStatus.Confirmed &&
                donorIdSet.Contains(d.DonorUserId);

            AddInclude(d => d.DonorUser);
        }
    }




    // ══════════════════════════════════════════════════════════════════════════
    // HOSPITAL ADMIN Specifications
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Fetches all users with Role == HospitalAdmin,
    /// includes Hospital nav-prop for linked hospital data.
    /// Ordered by CreatedAt DESC (newest first).
    /// </summary>
    public class AllHospitalAdminsSpecification : BaseSpecification<ApplicationUser>
    {
        public AllHospitalAdminsSpecification()
        {
            Criteria = u => u.Role == BloodDonationSystem.Enums.Role.HospitalAdmin;
            AddInclude(u => u.Hospital);
            ApplyOrderByDesc(u => u.CreatedAt);
        }
    }

    /// <summary>
    /// Fetch a single HospitalAdmin by Id, includes Hospital.
    /// </summary>
    public class HospitalAdminByIdSpecification : BaseSpecification<ApplicationUser>
    {
        public HospitalAdminByIdSpecification(string userId)
        {
            Criteria = u =>
                u.Id == userId &&
                u.Role == BloodDonationSystem.Enums.Role.HospitalAdmin;
            AddInclude(u => u.Hospital);
        }
    }
}
