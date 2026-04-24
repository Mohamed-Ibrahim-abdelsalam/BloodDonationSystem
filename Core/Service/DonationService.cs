using AutoMapper;
using BloodDonationSystem.Enums;
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
    public class DonationService : IDonationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DonationService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // ── POST /api/donations ───────────────────────────────────────────────
        public async Task<DonationResponseDto> CreateAsync(CreateDonationDto dto, string userId)
        {
            // ── Validate donor data ───────────────────────────────────────────
            if (dto.Age < 18 || dto.Age > 60)
                throw new ArgumentException("Age must be between 18 and 60.");

            if (dto.Weight < 50)
                throw new ArgumentException("Weight must be at least 50 kg.");

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ArgumentException("Address is required.");

            // ── Request-based donation validation ─────────────────────────────
            if (dto.BloodRequestId.HasValue)
            {
                var requestSpec = new BloodRequestByIdSpecification(dto.BloodRequestId.Value);
                var bloodRequest = await _uow.BloodRequests.GetEntityWithSpecAsync(requestSpec);

                // 404 — request not found
                if (bloodRequest is null)
                    throw new KeyNotFoundException(
                        $"Blood request with id {dto.BloodRequestId.Value} was not found.");

                // 400 — request not open
                if (bloodRequest.Status != BloodRequestStatus.Open)
                    throw new InvalidOperationException(
                        "Cannot donate to a closed or completed blood request.");

                // 400 — duplicate donation (same user + same request)
                var duplicateSpec = new DuplicateDonationSpecification(
                    userId, dto.BloodRequestId.Value);
                var existing = await _uow.Donations.GetEntityWithSpecAsync(duplicateSpec);

                if (existing is not null)
                    throw new InvalidOperationException(
                        "You have already submitted a donation for this request.");
            }

            // ── Build and save donation ───────────────────────────────────────
            var donation = _mapper.Map<Donation>(dto);
            donation.DonorUserId = userId;
            donation.Status = DonationStatus.Pending;
            donation.CreatedAt = DateTime.UtcNow;

            await _uow.Donations.AddAsync(donation);
            await _uow.SaveChangesAsync();

            // Reload with includes for full mapping
            var spec = new DonationByIdSpecification(donation.Id);
            var created = await _uow.Donations.GetEntityWithSpecAsync(spec);

            var result = _mapper.Map<DonationResponseDto>(created!);
            result.Message = "Donation created successfully";
            return result;
        }

        // ── GET /api/donations/my ─────────────────────────────────────────────
        public async Task<IEnumerable<MyDonationDto>> GetMyDonationsAsync(string userId)
        {
            var spec = new DonationsByUserSpecification(userId);
            var donations = await _uow.Donations.GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<MyDonationDto>>(donations);
        }

        // ── POST /api/donations/{id}/cancel ───────────────────────────────────
        public async Task CancelAsync(int donationId, string userId)
        {
            var spec = new DonationByIdSpecification(donationId);
            var donation = await _uow.Donations.GetEntityWithSpecAsync(spec);

            // 404
            if (donation is null)
                throw new KeyNotFoundException($"Donation with id {donationId} was not found.");

            // 403 — not the owner
            if (donation.DonorUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to cancel this donation.");

            // 400 — only Pending can be cancelled
            if (donation.Status != DonationStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending donations can be cancelled.");

            donation.Status = DonationStatus.Cancelled;
            _uow.Donations.Update(donation);
            await _uow.SaveChangesAsync();
        }
    }
}
