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
    public class HospitalInventoryService : IHospitalInventoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public HospitalInventoryService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // ── GET /api/hospital/inventory ───────────────────────────────────────
        public async Task<HospitalInventoryResponseDto> GetInventoryAsync(string userId)
        {
            // ── 1. Resolve Hospital Admin ─────────────────────────────────────
            var admin = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (!admin.HospitalId.HasValue)
                throw new InvalidOperationException(
                    "Your account is not linked to any hospital. " +
                    "Please contact the system administrator.");

            // ── 2. Validate hospital exists ───────────────────────────────────
            var hospitalSpec = new HospitalByIdSpecification(admin.HospitalId.Value);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(hospitalSpec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {admin.HospitalId.Value} was not found.");

            // ── 3. Load all AVAILABLE blood bags for this hospital ────────────
            // Spec: Status == Available, ordered by ExpiryDate ASC (FIFO)
            // Withdrawn bags are automatically excluded by the spec.
            var bagSpec = new AvailableBloodBagsByHospitalSpecification(admin.HospitalId.Value);
            var bags = (await _uow.BloodBags.GetAllWithSpecAsync(bagSpec)).ToList();

            // ── 4. Group by BloodType → build one InventoryItemDto per type ───
            // quantity    = count of available bags for that type
            // nearestExpiry = Min(ExpiryDate) within the group (FIFO — oldest first)
            // status      = computed from quantity
            var items = bags
                .GroupBy(b => b.BloodType)
                .Select(group =>
                {
                    var quantity = group.Count();
                    var status = ComputeStatus(quantity);

                    return new InventoryItemDto
                    {
                        BloodType = FormatBloodType(group.Key),
                        Quantity = quantity,
                        NearestExpiryDate = group.Min(b => b.ExpiryDate),
                        Status = status,
                    };
                })
                .OrderBy(i => i.BloodType)   // consistent display order
                .ToList();

            // ── 5. Build dashboard from the grouped items (zero extra queries) ─
            var dashboard = new InventoryDashboardDto
            {
                TotalUnits = items.Sum(i => i.Quantity),
                High = items.Count(i => i.Status == "High"),
                Low = items.Count(i => i.Status == "Low"),
                Critical = items.Count(i => i.Status == "Critical"),
            };

            return new HospitalInventoryResponseDto
            {
                HospitalId = hospital.Id,
                HospitalName = hospital.Name,
                Dashboard = dashboard,
                Inventory = items,
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Calculates inventory status from quantity.
        /// >= 10 → High | 3–9 → Low | &lt; 3 → Critical
        /// </summary>
        private static string ComputeStatus(int quantity) => quantity switch
        {
            >= 10 => "High",
            >= 3 => "Low",
            _ => "Critical",
        };

        /// <summary>
        /// Formats BloodType enum to the display string.
        /// OPositive → "O+" | ABNegative → "AB-"
        /// </summary>
        private static string FormatBloodType(BloodType bt) => bt.ToString()
            .Replace("Positive", "+")
            .Replace("Negative", "-");
    }
}
