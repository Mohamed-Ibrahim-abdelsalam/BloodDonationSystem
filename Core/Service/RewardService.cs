using AutoMapper;
using BloodDonationSystem.Models;
using DomainLayer.Enums;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class RewardService : IRewardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public RewardService(
            IUnitOfWork uow,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
        }

        // ── GET /api/rewards ──────────────────────────────────────────────────
        public async Task<IEnumerable<RewardDto>> GetAvailableRewardsAsync()
        {
            var spec = new AvailableRewardsSpecification();
            var rewards = await _uow.Rewards.GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<RewardDto>>(rewards);
        }

        // ── GET /api/rewards/{id} ─────────────────────────────────────────────
        public async Task<RewardDetailDto> GetByIdAsync(int rewardId)
        {
            var spec = new RewardByIdSpecification(rewardId);
            var reward = await _uow.Rewards.GetEntityWithSpecAsync(spec);

            if (reward is null)
                throw new KeyNotFoundException($"Reward with id {rewardId} was not found.");

            return _mapper.Map<RewardDetailDto>(reward);
        }

        // ── POST /api/rewards/redeem ──────────────────────────────────────────
        public async Task<RedeemResponseDto> RedeemAsync(int rewardId, string userId)
        {
            // Get reward
            var spec = new RewardByIdSpecification(rewardId);
            var reward = await _uow.Rewards.GetEntityWithSpecAsync(spec);

            // 404
            if (reward is null)
                throw new KeyNotFoundException($"Reward with id {rewardId} was not found.");

            // 400 — reward not available
            if (!reward.IsAvailable)
                throw new InvalidOperationException("This reward is not currently available.");

            // Get user
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // 400 — not enough points
            if (user.Points < reward.PointsRequired)
                throw new InvalidOperationException(
                    $"Insufficient points. You have {user.Points} points but need {reward.PointsRequired}.");

            // Deduct points
            user.Points -= reward.PointsRequired;
            await _userManager.UpdateAsync(user);

            // Create UserReward record
            var userReward = new UserReward
            {
                UserId = userId,
                RewardId = rewardId,
                PointsUsed = reward.PointsRequired,
                Status = UserRewardStatus.Unused,
                RedeemedAt = DateTime.UtcNow,
            };

            await _uow.UserRewards.AddAsync(userReward);
            await _uow.SaveChangesAsync();

            return new RedeemResponseDto
            {
                RewardId = reward.Id,
                Title = reward.Title,
                PointsUsed = reward.PointsRequired,
                RemainingPoints = user.Points,
                RedeemedAt = userReward.RedeemedAt,
                Message = "Reward redeemed successfully",
            };
        }

        // ── GET /api/users/rewards ────────────────────────────────────────────
        public async Task<IEnumerable<UserRewardDto>> GetMyRewardsAsync(string userId)
        {
            var spec = new UserRewardsByUserSpecification(userId);
            var userRewards = await _uow.UserRewards.GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<UserRewardDto>>(userRewards);
        }
    }
}
