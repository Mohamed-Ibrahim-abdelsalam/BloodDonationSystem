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

        public BloodRequestService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // ── POST /api/requests ────────────────────────────────────────────────
        public async Task<BloodRequestDto> CreateAsync(CreateBloodRequestDto dto, string userId)
        {
            // Validate Quantity
            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            // Validate NeededBy is in the future
            if (dto.NeededBy.HasValue && dto.NeededBy.Value <= DateTime.UtcNow)
                throw new ArgumentException("NeededBy must be a future date.");

            // Validate BloodType is a valid enum value
            if (!Enum.IsDefined(typeof(BloodType), dto.BloodType))
                throw new ArgumentException("Invalid BloodType value.");

            // Validate Priority is a valid enum value
            if (!Enum.IsDefined(typeof(RequestPriority), dto.Priority))
                throw new ArgumentException("Invalid Priority value.");

            var request = _mapper.Map<BloodRequest>(dto);
            request.RequestedByUserId = userId;
            request.Status = BloodRequestStatus.Open;
            request.CreatedAt = DateTime.UtcNow;

            await _uow.BloodRequests.AddAsync(request);
            await _uow.SaveChangesAsync();

            // Reload with user included for mapping
            var spec = new BloodRequestByIdSpecification(request.Id);
            var created = await _uow.BloodRequests.GetEntityWithSpecAsync(spec);

            var result = _mapper.Map<BloodRequestDto>(created);
            result.Message = "Blood request created successfully";
            return result;
        }

        // ── GET /api/requests ─────────────────────────────────────────────────
        public async Task<IEnumerable<BloodRequestDto>> GetAllAsync(BloodRequestQueryParams queryParams)
        {
            var spec = new BloodRequestSpecification(
                               queryParams.BloodType,
                               queryParams.Priority,
                               queryParams.Search);

            var requests = await _uow.BloodRequests.GetAllWithSpecAsync(spec);

            // Secondary sort: after Emergency first, sort by CreatedAt descending
            var sorted = requests
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.CreatedAt);

            return _mapper.Map<IEnumerable<BloodRequestDto>>(sorted);
        }

        // ── GET /api/requests/{id} ────────────────────────────────────────────
        public async Task<BloodRequestDetailDto> GetByIdAsync(int id)
        {
            var spec = new BloodRequestByIdSpecification(id);
            var request = await _uow.BloodRequests.GetEntityWithSpecAsync(spec);

            if (request is null)
                throw new KeyNotFoundException($"Blood request with id {id} was not found.");

            return _mapper.Map<BloodRequestDetailDto>(request);
        }

        // ── GET /api/requests/my ──────────────────────────────────────────────
        public async Task<IEnumerable<MyBloodRequestDto>> GetMyRequestsAsync(string userId)
        {
            var spec = new BloodRequestByUserSpecification(userId);
            var requests = await _uow.BloodRequests.GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<MyBloodRequestDto>>(requests);
        }

        // ── DELETE /api/requests/{id} ─────────────────────────────────────────
        public async Task DeleteAsync(int id, string userId)
        {
            var spec = new BloodRequestByIdSpecification(id);
            var request = await _uow.BloodRequests.GetEntityWithSpecAsync(spec);

            // 404
            if (request is null)
                throw new KeyNotFoundException($"Blood request with id {id} was not found.");

            // 403 — only owner can delete
            if (request.RequestedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to delete this request.");

            // 400 — only Open requests can be deleted
            if (request.Status != BloodRequestStatus.Open)
                throw new InvalidOperationException(
                    "Only open blood requests can be deleted.");

            _uow.BloodRequests.Delete(request);
            await _uow.SaveChangesAsync();
        }
    }  
}
