using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Enums;
using DomainLayer.Interfaces;
using DomainLayer.Models;
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
    public class QrService : IQrService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        private const int QrExpiryMinutes = 15;

        public QrService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // ── GET /api/donations/{id}/qr ────────────────────────────────────────
        public async Task<QrTokenResponseDto> GenerateDonationQrAsync(int donationId, string userId)
        {
            var donationSpec = new DonationByIdSpecification(donationId);
            var donation = await _uow.Donations.GetEntityWithSpecAsync(donationSpec);

            // 404
            if (donation is null)
                throw new KeyNotFoundException($"Donation with id {donationId} was not found.");

            // 403 — must be the donor
            if (donation.DonorUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to generate a QR for this donation.");

            // 400 — only Pending donations can get a QR
            if (donation.Status != DonationStatus.Pending)
                throw new InvalidOperationException(
                    "QR can only be generated for pending donations.");

            // If an active token already exists, return it instead of creating a new one
            var existingSpec = new ActiveDonationQrSpecification(donationId);
            var existingToken = await _uow.QrTokens.GetEntityWithSpecAsync(existingSpec);
            if (existingToken is not null)
            {
                return MapToQrResponse(existingToken, donationId);
            }

            // Generate new token
            var qrToken = new QrToken
            {
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                Type = QrTokenType.Donation,
                DonationId = donationId,
                ExpiryDate = DateTime.UtcNow.AddMinutes(QrExpiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _uow.QrTokens.AddAsync(qrToken);
            await _uow.SaveChangesAsync();

            return MapToQrResponse(qrToken, donationId);
        }

        // ── GET /api/requests/{id}/pickup-qr ─────────────────────────────────
        public async Task<QrTokenResponseDto> GeneratePickupQrAsync(int requestId, string userId)
        {
            var requestSpec = new BloodRequestByIdSpecification(requestId);
            var bloodRequest = await _uow.BloodRequests.GetEntityWithSpecAsync(requestSpec);

            // 404
            if (bloodRequest is null)
                throw new KeyNotFoundException($"Blood request with id {requestId} was not found.");

            // 400 — request must be Fulfilled (donation confirmed) to generate pickup QR
            if (bloodRequest.Status != BloodRequestStatus.Fulfilled)
                throw new InvalidOperationException(
                    "Pickup QR can only be generated for fulfilled requests (after donation is confirmed).");

            // If an active token already exists, return it
            var existingSpec = new ActivePickupQrSpecification(requestId);
            var existingToken = await _uow.QrTokens.GetEntityWithSpecAsync(existingSpec);
            if (existingToken is not null)
            {
                return MapToQrResponse(existingToken, requestId);
            }

            var qrToken = new QrToken
            {
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                Type = QrTokenType.Pickup,
                BloodRequestId = requestId,
                ExpiryDate = DateTime.UtcNow.AddMinutes(QrExpiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _uow.QrTokens.AddAsync(qrToken);
            await _uow.SaveChangesAsync();

            return MapToQrResponse(qrToken, requestId);
        }




        // ── GET /api/hospital/donations/{id}/pickup-qr ───────────────────────
        // HospitalAdmin generates a Pickup QR for a general donation (no BloodRequest)
        // This enables Case 2 of ScanPickupQr — withdrawing blood from general stock
        public async Task<QrTokenResponseDto> GenerateGeneralDonationPickupQrAsync(
            int donationId, string hospitalAdminId)
        {
            // Verify admin and their hospital
            var admin = await _userManager.FindByIdAsync(hospitalAdminId)
                ?? throw new KeyNotFoundException("Hospital admin not found.");

            if (!admin.HospitalId.HasValue)
                throw new InvalidOperationException(
                    "Your account is not linked to any hospital.");

            var donationSpec = new DonationByIdSpecification(donationId);
            var donation = await _uow.Donations.GetEntityWithSpecAsync(donationSpec)
                ?? throw new KeyNotFoundException($"Donation with id {donationId} was not found.");

            // Must be a general donation (no BloodRequest linked)
            if (donation.BloodRequestId.HasValue)
                throw new InvalidOperationException(
                    "This donation is linked to a blood request. " +
                    "Use the request pickup QR instead.");

            // Must be confirmed (blood bag exists in inventory)
            if (donation.Status != DonationStatus.Confirmed)
                throw new InvalidOperationException(
                    "Pickup QR can only be generated for confirmed donations.");

            // Must belong to admin's hospital
            if (donation.HospitalId != admin.HospitalId.Value)
                throw new UnauthorizedAccessException(
                    "This donation does not belong to your hospital.");

            // Return existing active token if available
            var existingSpec = new ActiveDonationPickupQrSpecification(donationId);
            var existingToken = await _uow.QrTokens.GetEntityWithSpecAsync(existingSpec);
            if (existingToken is not null)
                return MapToQrResponse(existingToken, donationId);

            var qrToken = new QrToken
            {
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                Type = QrTokenType.Pickup,
                DonationId = donationId,      // linked to donation, NOT a BloodRequest
                BloodRequestId = null,
                ExpiryDate = DateTime.UtcNow.AddMinutes(QrExpiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _uow.QrTokens.AddAsync(qrToken);
            await _uow.SaveChangesAsync();

            return MapToQrResponse(qrToken, donationId);
        }




        // ── POST /api/hospital/donations/{id}/scan ────────────────────────────
        public async Task<DonationScanResponseDto> ScanDonationQrAsync(
            string qrToken, string hospitalAdminId)
        {
            var tokenSpec = new QrTokenByValueSpecification(qrToken);
            var token = await _uow.QrTokens.GetEntityWithSpecAsync(tokenSpec);


            // Verify the scanning admin belongs to the same hospital as the donation\n'
            var admin = await _userManager.FindByIdAsync(hospitalAdminId)
                   ?? throw new KeyNotFoundException("Hospital admin not found.");
       
            if (!admin.HospitalId.HasValue)
                throw new InvalidOperationException("Your account is not linked to any hospital.");

            // 404
            if (token is null)
                throw new KeyNotFoundException("QR token not found.");

            // 400 — wrong type
            if (token.Type != QrTokenType.Donation)
                throw new InvalidOperationException("Invalid QR token type. Expected a Donation QR.");

            // 400 — expired
            if (token.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("QR token has expired.");

            // 400 — already used
            if (token.IsUsed)
                throw new InvalidOperationException("QR token has already been used.");

            // 400 — token doesn't match route id
            // Extract donationId from the token itself — no route id needed\n'
             if (!token.DonationId.HasValue)
                throw new InvalidOperationException("QR token is not linked to any donation.");
    
                var donationId = token.DonationId.Value;

            // Mark token as used
            token.IsUsed = true;
            _uow.QrTokens.Update(token);

            // Update donation status → Confirmed
            var donationSpec = new DonationByIdSpecification(donationId);
            var donation = await _uow.Donations.GetEntityWithSpecAsync(donationSpec)
                ?? throw new KeyNotFoundException($"Donation with id {donationId} was not found.");
            // Verify donation belongs to admin\'s hospital\n'
                        if (donation.HospitalId.HasValue &&
                            donation.HospitalId.Value != admin.HospitalId.Value)
                            throw new UnauthorizedAccessException(
                                "This donation is not assigned to your hospital.");
            
              donation.Status = DonationStatus.Confirmed;
              donation.ConfirmedAt = DateTime.UtcNow;
                _uow.Donations.Update(donation); //////////////////////

            // ── FIX 1: Quantity-aware Fulfillment ────────────────────────────────
            // Only mark BloodRequest as Fulfilled when confirmed donation count
            // reaches the requested quantity — not just on the first donation.
            if (donation.BloodRequestId.HasValue)
            {
                var requestSpec = new BloodRequestByIdSpecification(donation.BloodRequestId.Value);
                var bloodRequest = await _uow.BloodRequests.GetEntityWithSpecAsync(requestSpec);
                if (bloodRequest is not null && bloodRequest.Status == BloodRequestStatus.Open)
                {
                    // Count all confirmed donations for this request AFTER this one is saved.
                    // We add 1 manually because the current donation is Confirmed in memory
                    // but SaveChangesAsync hasn't run yet.
                    var countSpec = new ConfirmedDonationsByRequestSpecification(donation.BloodRequestId.Value);
                    var confirmedSoFar = await _uow.Donations.CountAsync(countSpec);
                    var totalConfirmed = confirmedSoFar + 1; // include the current donation

                    if (totalConfirmed >= bloodRequest.Quantity)
                    {
                        bloodRequest.Status = BloodRequestStatus.Fulfilled;
                        _uow.BloodRequests.Update(bloodRequest);
                    }
                }
            }

            // Award points to donor (+50 per confirmed donation)
            var donor = await _userManager.FindByIdAsync(donation.DonorUserId);
            if (donor is not null)
            {
                donor.Points += 50;
                await _userManager.UpdateAsync(donor);
            }

            // ── Auto-create BloodBag on successful donation scan ──────────────
            // CreatedAt = scan timestamp | ExpiryDate = CreatedAt + 42 days
            if (donation.HospitalId.HasValue)
            {
                var scanTime = donation.ConfirmedAt ?? DateTime.UtcNow;
                var bloodBag = new BloodBag
                {
                    DonationId = donationId,
                    HospitalId = donation.HospitalId.Value,
                    BloodType = donation.BloodType,
                    Status = BloodBagStatus.Available,
                    CreatedAt = scanTime,
                    ExpiryDate = scanTime.AddDays(42),
                };
                await _uow.BloodBags.AddAsync(bloodBag);
            }

            await _uow.SaveChangesAsync();

            return new DonationScanResponseDto
            {
                Message = "Donation received successfully",
                DonationId = donationId,
                Status = DonationStatus.Confirmed.ToString(),
            };
        }

       
        // ── POST /api/requests/{id}/pickup-scan ───────────────────────────────
        // Supports TWO cases:
        //   Case 1 — BloodRequest pickup : token.BloodRequestId is set
        //            → User (owner) OR HospitalAdmin can scan
        //            → BloodRequest.Status = Completed
        //
        //   Case 2 — General donation withdrawal : token.DonationId is set, no BloodRequest
        //            → HospitalAdmin only
        //            → Donation.Status = Withdrawn
        public async Task<PickupScanResponseDto> ScanPickupQrAsync(
             string qrToken, string userId, string userRole)
        {
            // ── 1. Validate QR token ──────────────────────────────────────────
            var tokenSpec = new QrTokenByValueSpecification(qrToken);
            var token = await _uow.QrTokens.GetEntityWithSpecAsync(tokenSpec);

            if (token is null)
                throw new KeyNotFoundException("QR token not found.");

            if (token.Type != QrTokenType.Pickup)
                throw new InvalidOperationException("Invalid QR token type. Expected a Pickup QR.");

            if (token.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("QR token has expired.");

            if (token.IsUsed)
                throw new InvalidOperationException("QR token has already been used.");

            // ── 2. Route to correct case based on what the token is linked to ─
            bool isHospitalAdmin = userRole == "HospitalAdmin";

            // ── CASE 1: Token linked to a BloodRequest ────────────────────────
            if (token.BloodRequestId.HasValue)
            {
                var requestSpec = new BloodRequestByIdSpecification(token.BloodRequestId.Value);
                var bloodRequest = await _uow.BloodRequests.GetEntityWithSpecAsync(requestSpec)
                    ?? throw new KeyNotFoundException(
                        $"Blood request with id {token.BloodRequestId.Value} was not found.");

                // Authorization: request owner OR HospitalAdmin
                bool isOwner = bloodRequest.RequestedByUserId == userId;
                if (!isOwner && !isHospitalAdmin)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to confirm this blood pickup. " +
                        "Only the request owner or a Hospital Admin can scan this QR.");

                // Mark token used
                token.IsUsed = true;
                _uow.QrTokens.Update(token);

                // Complete the request
                bloodRequest.Status = BloodRequestStatus.Completed;
                bloodRequest.IsBloodReceived = true;
                _uow.BloodRequests.Update(bloodRequest);

                await _uow.SaveChangesAsync();

                return new PickupScanResponseDto
                {
                    Message = "Blood pickup confirmed successfully",
                    RequestId = bloodRequest.Id,
                    Status = BloodRequestStatus.Completed.ToString(),
                };
            }

            // ── CASE 2: Token linked to a General Donation (no BloodRequest) ──
            if (token.DonationId.HasValue)
            {
                // Only HospitalAdmin can withdraw from general stock
                if (!isHospitalAdmin)
                    throw new UnauthorizedAccessException(
                        "Only a Hospital Admin can confirm withdrawal of a general donation.");

                var donationSpec = new DonationByIdSpecification(token.DonationId.Value);
                var donation = await _uow.Donations.GetEntityWithSpecAsync(donationSpec)
                    ?? throw new KeyNotFoundException(
                        $"Donation with id {token.DonationId.Value} was not found.");

                // Mark token used
                token.IsUsed = true;
                _uow.QrTokens.Update(token);

                // Mark donation as Withdrawn from hospital stock
                donation.Status = DonationStatus.Withdrawn;
                _uow.Donations.Update(donation);

                // ── FIX 2: Mark the corresponding BloodBag as Withdrawn ───────
                // Without this the blood bag stays "Available" in inventory
                // even though the physical bag has left the hospital.
                var bagSpec = new BloodBagByDonationSpecification(donation.Id);
                var bloodBag = await _uow.BloodBags.GetEntityWithSpecAsync(bagSpec);
                if (bloodBag is not null)
                {
                    bloodBag.Status = BloodBagStatus.Withdrawn;
                    _uow.BloodBags.Update(bloodBag);
                }

                await _uow.SaveChangesAsync();

                return new PickupScanResponseDto
                {
                    Message = "Blood bag withdrawn successfully",
                    DonationId = donation.Id,
                    Status = DonationStatus.Withdrawn.ToString(),
                };
            }

            // ── Fallback: token has neither BloodRequestId nor DonationId ─────
            throw new InvalidOperationException(
                "QR token is not linked to any blood request or donation.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static QrTokenResponseDto MapToQrResponse(QrToken token, int referenceId)
            => new QrTokenResponseDto
            {
                QrToken = token.Token,
                QrType = token.Type.ToString(),
                ReferenceId = referenceId,
                ExpiresAt = token.ExpiryDate,
            };
    }
}
