using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class RewardAdminService : IRewardAdminService
    {
        private readonly IUnitOfWork _uow;

        public RewardAdminService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ── POST /api/admin/rewards ───────────────────────────────────────────
        public async Task<AdminRewardDto> CreateAsync(CreateRewardDto dto)
        {
            // Validate unique title
            var titleSpec = new RewardByTitleSpecification(dto.Title);
            var existing = await _uow.Rewards.GetEntityWithSpecAsync(titleSpec);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A reward with title '{dto.Title}' already exists.");

            var now = DateTime.UtcNow;
            var reward = new Reward
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                PointsRequired = dto.PointsRequired,
                IsAvailable = true,
                CreatedAt = now,
            };

            await _uow.Rewards.AddAsync(reward);
            await _uow.SaveChangesAsync();

            return ToDto(reward, "Reward created successfully");
        }

        // ── PUT /api/admin/rewards/{id} ───────────────────────────────────────
        public async Task<AdminRewardDto> UpdateAsync(int id, UpdateRewardDto dto)
        {
            // Validate reward exists
            var spec = new RewardByIdSpecification(id);
            var reward = await _uow.Rewards.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException(
                    $"Reward with id {id} was not found.");

            // Validate unique title (exclude self)
            var titleSpec = new RewardByTitleSpecification(dto.Title, excludeId: id);
            var duplicate = await _uow.Rewards.GetEntityWithSpecAsync(titleSpec);
            if (duplicate is not null)
                throw new InvalidOperationException(
                    $"A reward with title '{dto.Title}' already exists.");

            reward.Title = dto.Title.Trim();
            reward.Description = dto.Description?.Trim();
            reward.PointsRequired = dto.PointsRequired;

            _uow.Rewards.Update(reward);
            await _uow.SaveChangesAsync();

            return ToDto(reward, "Reward updated successfully", updatedAt: DateTime.UtcNow);
        }

        // ── DELETE /api/admin/rewards/{id} ────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            var spec = new RewardByIdSpecification(id);
            var reward = await _uow.Rewards.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException(
                    $"Reward with id {id} was not found.");

            _uow.Rewards.Delete(reward);
            await _uow.SaveChangesAsync();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static AdminRewardDto ToDto(
            Reward reward,
            string? message = null,
            DateTime? updatedAt = null) => new()
            {
                Id = reward.Id,
                Title = reward.Title,
                Description = reward.Description,
                PointsRequired = reward.PointsRequired,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = updatedAt,
                Message = message,
            };
    }
}
