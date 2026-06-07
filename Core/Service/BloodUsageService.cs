using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
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
    public class BloodUsageService : IBloodUsageService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        // All valid period strings → days to subtract from today
        private static readonly Dictionary<string, int> PeriodDays = new()
        {
            { "7days",   7   },
            { "1month",  30  },
            { "3months", 90  },
            { "6months", 180 },
        };

        public BloodUsageService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // ── GET /api/hospital/blood-usage ─────────────────────────────────────
        public async Task<BloodUsageResponseDto> GetBloodUsageAsync(
            string userId, string period)
        {
            // ── 1. Validate period ────────────────────────────────────────────
            var normalised = period.ToLower().Trim();
            if (!PeriodDays.TryGetValue(normalised, out var days))
                throw new ArgumentException(
                    $"Invalid period '{period}'. Allowed values: 7days, 1month, 3months, 6months.");

            // ── 2. Resolve Hospital Admin ─────────────────────────────────────
            var admin = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (!admin.HospitalId.HasValue)
                throw new InvalidOperationException(
                    "Your account is not linked to any hospital.");

            // ── 3. Validate hospital exists ───────────────────────────────────
            var hospitalSpec = new HospitalByIdSpecification(admin.HospitalId.Value);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(hospitalSpec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {admin.HospitalId.Value} was not found.");

            // ── 4. Determine date range ───────────────────────────────────────
            var dateFrom = DateTime.UtcNow.Date.AddDays(-days);

            // ── 5. Fetch withdrawn bags within the period ─────────────────────
            var spec = new WithdrawnBloodBagsByPeriodSpecification(
                admin.HospitalId.Value, dateFrom);

            var withdrawnBags = (await _uow.BloodBags.GetAllWithSpecAsync(spec)).ToList();

            // ── 6. Group by BloodType and count ──────────────────────────────
            var totalUsed = withdrawnBags.Count;

            var grouped = withdrawnBags
                .GroupBy(b => b.BloodType)
                .Select(g => new
                {
                    BloodType = g.Key,
                    UsedUnits = g.Count(),
                })
                .OrderByDescending(g => g.UsedUnits)
                .ToList();

            // ── 7. Calculate percentage per blood type ────────────────────────
            var usageItems = grouped.Select(g => new BloodUsageItemDto
            {
                BloodType = FormatBloodType(g.BloodType),
                UsedUnits = g.UsedUnits,
                Percentage = totalUsed > 0
                    ? Math.Round((g.UsedUnits / (double)totalUsed) * 100, 1)
                    : 0,
            }).ToList();

            return new BloodUsageResponseDto
            {
                HospitalId = hospital.Id,
                HospitalName = hospital.Name,
                Period = normalised,
                TotalUsedUnits = totalUsed,
                BloodUsage = usageItems,
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Formats BloodType enum to the display string.
        /// OPositive → "OPositive" (kept as-is per spec response format)
        /// </summary>
        private static string FormatBloodType(BloodType bt)
            => bt.ToString().Replace("_", string.Empty);
    }
}
