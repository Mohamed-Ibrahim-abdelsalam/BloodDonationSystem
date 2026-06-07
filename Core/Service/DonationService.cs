using AutoMapper;
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
    public class DonationService : IDonationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public DonationService(
            IUnitOfWork uow,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // ── POST /api/donations ───────────────────────────────────────────────
        public async Task<DonationResponseDto> CreateAsync(CreateDonationDto dto, string userId)
        {
            // ── 1. Resolve authenticated user (source of BloodType) ───────────
            var donor = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("Authenticated user was not found.");

            // ── 2. Validate hospital exists (always required) ─────────────────
            var hospitalSpec = new HospitalByIdSpecification(dto.HospitalId);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(hospitalSpec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {dto.HospitalId} was not found.");



            // ── 3. Request-based donation extra validations ───────────────────\n'
                    // bloodRequest is hoisted outside the if-block so Step 5\n'
                // can reuse it for the notification without a second DB query.\n'
                BloodRequest? bloodRequest = null;
    
                if (dto.BloodRequestId.HasValue)
                {
                    var requestSpec = new BloodRequestByIdSpecification(dto.BloodRequestId.Value);
                    bloodRequest = await _uow.BloodRequests.GetEntityWithSpecAsync(requestSpec)
                        ?? throw new KeyNotFoundException(
                            $"Blood request with id {dto.BloodRequestId.Value} was not found.");
    
                    // 400 — request must be Open\n'
                    if (bloodRequest.Status != BloodRequestStatus.Open)
                       throw new InvalidOperationException(
                          "Cannot donate to a blood request that is not Open.");
    
                    // 400 — hospital selected must match the request\'s hospital
                    if (bloodRequest.HospitalId.HasValue &&
                        bloodRequest.HospitalId.Value != dto.HospitalId)
                        throw new InvalidOperationException(
                            "The selected hospital does not match the blood request\'s hospital." +
                            $"Please select hospital id {bloodRequest.HospitalId.Value}.");
    
                    // 400 — prevent duplicate (same user + same request)\n'
                    var duplicateSpec = new DuplicateDonationSpecification(
                        userId, dto.BloodRequestId.Value);
                    var existing = await _uow.Donations.GetEntityWithSpecAsync(duplicateSpec);
    
                    if (existing is not null)
                        throw new InvalidOperationException(
                            "You have already submitted a donation for this blood request.");
                }

            // ── 4. Build the Donation entity ──────────────────────────────────
            // BloodType comes from the authenticated user — never from the request body.
            // Address comes from the authenticated user's profile.
            // MedicalCondition (bool) is persisted as string in the existing DB column.
            var donation = new Donation
            {
                DonorUserId = userId,
                BloodRequestId = dto.BloodRequestId,
                HospitalId = dto.HospitalId,
                BloodType = donor.BloodType,           // ← from user profile
                Age = dto.Age,
                Weight = dto.Weight,
                HasTattoo = dto.HasTattoo,
                LastDonationDate = dto.LastDonationDate,
                Address = donor.Address,             // ← from user profile
                MedicalCondition = dto.MedicalCondition.ToString(), // bool → string (existing schema)
                Status = DonationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };

            await _uow.Donations.AddAsync(donation);
            await _uow.SaveChangesAsync();

            // ── 5. Notify the request owner (request-linked donations only) ────\n'
           // Reuse bloodRequest already loaded in Step 3 — no extra DB query.\n'
           // General donations (BloodRequestId == null) skip this block.\n'
                if (bloodRequest is not null)
                {
                    try
                    {
                        await _notificationService.SendAsync(
                            receiverUserId: bloodRequest.RequestedByUserId,
                            title:          "Donation Received",
                            message:        "A donor has volunteered to donate blood for your request.",
                            referenceId:    bloodRequest.Id,
                            referenceType:  "BloodRequest");
                    }
                    catch
                    {
                        // Notification failure is non-critical — donation flow continues\n'
                    }
                }

            // ── 6. Reload with navigation properties for full response ─────────\n'
               var reloadSpec = new DonationByIdSpecification(donation.Id);
              var created    = await _uow.Donations.GetEntityWithSpecAsync(reloadSpec);

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
                throw new KeyNotFoundException(
                    $"Donation with id {donationId} was not found.");

            // 403 — caller must be the owner
            if (donation.DonorUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to cancel this donation.");

            // 400 — only Pending donations can be cancelled
            if (donation.Status != DonationStatus.Pending)
                throw new InvalidOperationException(
                    "Only donations with status 'Pending' can be cancelled.");

            donation.Status = DonationStatus.Cancelled;
            _uow.Donations.Update(donation);
            await _uow.SaveChangesAsync();
        }
    }
}
