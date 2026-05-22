using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    /// <summary>
    /// GET /api/rewards — available rewards ordered by PointsRequired ASC
    /// </summary>
    public class AvailableRewardsSpecification : BaseSpecification<Reward>
    {
        public AvailableRewardsSpecification()
        {
            Criteria = r => r.IsAvailable;
            ApplyOrderBy(r => r.PointsRequired);
        }
    }

    /// <summary>
    /// GET /api/rewards/{id}
    /// </summary>
    public class RewardByIdSpecification : BaseSpecification<Reward>
    {
        public RewardByIdSpecification(int id)
        {
            Criteria = r => r.Id == id;
        }
    }

    /// <summary>
    /// GET /api/users/rewards — user redemption history ordered by newest first
    /// Includes Reward for title
    /// </summary>
    public class UserRewardsByUserSpecification : BaseSpecification<UserReward>
    {
        public UserRewardsByUserSpecification(string userId)
        {
            AddInclude(ur => ur.Reward);
            Criteria = ur => ur.UserId == userId;
            ApplyOrderByDesc(ur => ur.RedeemedAt);
        }
    }




    /// <summary>
    /// Duplicate title check — finds any reward with the given title
    /// excluding a specific id (used during update to ignore self).
    /// </summary>
    public class RewardByTitleSpecification : BaseSpecification<Reward>
    {
        public RewardByTitleSpecification(string title, int excludeId = 0)
        {
            Criteria = r =>
                r.Title.ToLower() == title.ToLower() &&
                r.Id != excludeId;
        }
    }

    /// <summary>
    /// GET /api/admin/rewards — all rewards ordered by CreatedAt DESC.
    /// </summary>
    public class AllRewardsAdminSpecification : BaseSpecification<Reward>
    {
        public AllRewardsAdminSpecification()
        {
            ApplyOrderByDesc(r => r.CreatedAt);
        }
    }

}
