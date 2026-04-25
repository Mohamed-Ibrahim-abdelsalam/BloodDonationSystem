using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IRewardService
    {
        Task<IEnumerable<RewardDto>> GetAvailableRewardsAsync();
        Task<RewardDetailDto> GetByIdAsync(int rewardId);
        Task<RedeemResponseDto> RedeemAsync(int rewardId, string userId);
        Task<IEnumerable<UserRewardDto>> GetMyRewardsAsync(string userId);
    }
}
