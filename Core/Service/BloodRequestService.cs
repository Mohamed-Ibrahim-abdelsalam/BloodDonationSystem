using AutoMapper;
using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using ServiceAbstraction.Dtos.BloodRequests;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        // Threshold: if NeededBy is within this many days → Emergency
        private const int EmergencyThresholdDays = 3;

        public BloodRequestService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // ── POST /api/requests ────────────────────────────────────────────────
        public async Task<BloodRequestDto> CreateAsync(CreateBloodRequestDto dto, string userId)
        {
            // ── 1. Validate Hospital exists and load its name ─────────────────
            var hospitalSpec = new HospitalByIdSpecification(dto.HospitalId);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(hospitalSpec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {dto.HospitalId} was not found.");

            // ── 2. Validate NeededBy is a strictly future date (date only) ────
            var neededByDate = dto.NeededBy.Date;
            var todayUtc = DateTime.UtcNow.Date;

            if (neededByDate <= todayUtc)
                throw new ArgumentException(
                    "NeededBy must be a future date. Today or past dates are not allowed.");

            // ── 3. Validate Quantity ──────────────────────────────────────────
            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            // ── 4. Validate BloodType is a defined enum value ─────────────────
            if (!Enum.IsDefined(typeof(BloodType), dto.BloodType))
                throw new ArgumentException($"'{dto.BloodType}' is not a valid BloodType.");

            // ── 5. Auto-calculate Priority from NeededBy ──────────────────────
            var priority = CalculatePriority(neededByDate, todayUtc);

            // ── 6. Build the entity ───────────────────────────────────────────
            // hospitalName comes from DB — NOT from the request body
            var request = new BloodRequest
            {
                RequestedByUserId = userId,
                HospitalId = dto.HospitalId,
                HospitalName = hospital.Name,       // loaded from DB
                HospitalLocation = dto.HospitalLocation,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                BloodType = dto.BloodType,
                Quantity = dto.Quantity,
                Priority = priority,             // auto-calculated
                Status = BloodRequestStatus.Open,
                NeededBy = neededByDate,         // date only (no time)
                CreatedAt = DateTime.UtcNow,
            };

            await _uow.BloodRequests.AddAsync(request);
            await _uow.SaveChangesAsync();

            // ── 7. Reload with navigation properties for full response ─────────
            var spec = new BloodRequestByIdSpecification(request.Id);
            var created = await _uow.BloodRequests.GetEntityWithSpecAsync(spec);

            var result = _mapper.Map<BloodRequestDto>(created!);
            result.Message = "Blood request created successfully";
            return result;
        }


        // ── GET /api/requests/{id} ────────────────────────────────────────────
        public async Task<BloodRequestDetailDto> GetByIdAsync(int id)
        {
            var spec = new BloodRequestByIdSpecification(id);
            var request = await _uow.BloodRequests.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException(
                    $"Blood request with id {id} was not found.");

            // Recalculate priority before returning
            if (RefreshPriority(request))
                await _uow.SaveChangesAsync();

            return _mapper.Map<BloodRequestDetailDto>(request);
        }

        // ── GET /api/requests/my ──────────────────────────────────────────────
        public async Task<IEnumerable<MyBloodRequestDto>> GetMyRequestsAsync(string userId)
        {
            var spec = new BloodRequestByUserSpecification(userId);
            var requests = await _uow.BloodRequests.GetAllWithSpecAsync(spec);

            var changed = ApplyDynamicPriority(requests.ToList());
            if (changed)
                await _uow.SaveChangesAsync();

            return _mapper.Map<IEnumerable<MyBloodRequestDto>>(requests);
        }

        // ── DELETE /api/requests/{id} ─────────────────────────────────────────
        public async Task DeleteAsync(int id, string userId)
        {
            var spec = new BloodRequestByIdSpecification(id);
            var request = await _uow.BloodRequests.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException(
                    $"Blood request with id {id} was not found.");

            // 403 — only owner can delete
            if (request.RequestedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to delete this blood request.");

            // 400 — only Open requests can be deleted
            if (request.Status != BloodRequestStatus.Open)
                throw new InvalidOperationException(
                    "Only Open blood requests can be deleted.");

            _uow.BloodRequests.Delete(request);
            await _uow.SaveChangesAsync();
        }

        // ══════════════════════════════════════════════════════════════════════
        // Private helpers — Priority logic
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Calculates RequestPriority based on days remaining until NeededBy.
        /// ≤ 3 days → Emergency, otherwise → Normal.
        /// </summary>
        private static RequestPriority CalculatePriority(DateTime neededByDate, DateTime todayUtc)
        {
            var daysRemaining = (neededByDate - todayUtc).Days;
            return daysRemaining <= EmergencyThresholdDays
                ? RequestPriority.Emergency
                : RequestPriority.Normal;
        }

        /// <summary>
        /// Re-evaluates a single request's Priority.
        /// Only upgrades Normal → Emergency (never downgrades Emergency → Normal).
        /// Returns true when the entity was mutated and needs to be saved.
        /// </summary>
        private static bool RefreshPriority(BloodRequest request)
        {
            if (request.NeededBy is null) return false;
            if (request.Priority == RequestPriority.Emergency) return false;  // already highest
            if (request.Status != BloodRequestStatus.Open) return false;      // closed requests are not updated

            var todayUtc = DateTime.UtcNow.Date;
            var newPriority = CalculatePriority(request.NeededBy.Value.Date, todayUtc);

            if (newPriority == RequestPriority.Emergency)
            {
                request.Priority = RequestPriority.Emergency;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Runs RefreshPriority over a collection and marks changed entities
        /// via the UoW change tracker (EF Core tracks the mutated objects).
        /// Returns true if at least one entity was updated.
        /// </summary>
        private bool ApplyDynamicPriority(IList<BloodRequest> requests)
        {
            var anyChanged = false;
            foreach (var r in requests)
            {
                if (RefreshPriority(r))
                {
                    _uow.BloodRequests.Update(r);
                    anyChanged = true;
                }
            }
            return anyChanged;
        }
    }
}
