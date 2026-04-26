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

        // ── POST /api/hospital/donations/{id}/scan ────────────────────────────
        public async Task<DonationScanResponseDto> ScanDonationQrAsync(
            int donationId, string qrToken, string hospitalAdminId)
        {
            var tokenSpec = new QrTokenByValueSpecification(qrToken);
            var token = await _uow.QrTokens.GetEntityWithSpecAsync(tokenSpec);

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
            if (token.DonationId != donationId)
                throw new InvalidOperationException("QR token does not match the provided donation ID.");

            // Mark token as used
            token.IsUsed = true;
            _uow.QrTokens.Update(token);

            // Update donation status → Confirmed
            var donationSpec = new DonationByIdSpecification(donationId);
            var donation = await _uow.Donations.GetEntityWithSpecAsync(donationSpec)
                ?? throw new KeyNotFoundException($"Donation with id {donationId} was not found.");

            donation.Status = DonationStatus.Confirmed;
            donation.ConfirmedAt = DateTime.UtcNow;
            _uow.Donations.Update(donation);

            // Mark request as Fulfilled (ready for pickup)
            if (donation.BloodRequestId.HasValue)
            {
                var requestSpec = new BloodRequestByIdSpecification(donation.BloodRequestId.Value);
                var bloodRequest = await _uow.BloodRequests.GetEntityWithSpecAsync(requestSpec);
                if (bloodRequest is not null)
                {
                    bloodRequest.Status = BloodRequestStatus.Fulfilled;
                    _uow.BloodRequests.Update(bloodRequest);
                }
            }

            // Award points to donor (+50 per confirmed donation)
            var donor = await _userManager.FindByIdAsync(donation.DonorUserId);
            if (donor is not null)
            {
                donor.Points += 50;
                await _userManager.UpdateAsync(donor);
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
        public async Task<PickupScanResponseDto> ScanPickupQrAsync(
            int requestId, string qrToken, string userId)
        {
            var tokenSpec = new QrTokenByValueSpecification(qrToken);
            var token = await _uow.QrTokens.GetEntityWithSpecAsync(tokenSpec);

            // 404
            if (token is null)
                throw new KeyNotFoundException("QR token not found.");

            // 400 — wrong type
            if (token.Type != QrTokenType.Pickup)
                throw new InvalidOperationException("Invalid QR token type. Expected a Pickup QR.");

            // 400 — expired
            if (token.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("QR token has expired.");

            // 400 — already used
            if (token.IsUsed)
                throw new InvalidOperationException("QR token has already been used.");

            // 400 — token doesn't match route id
            if (token.BloodRequestId != requestId)
                throw new InvalidOperationException("QR token does not match the provided request ID.");

            // Get blood request with owner check
            var requestSpec = new BloodRequestByIdSpecification(requestId);
            var bloodRequest = await _uow.BloodRequests.GetEntityWithSpecAsync(requestSpec)
                ?? throw new KeyNotFoundException($"Blood request with id {requestId} was not found.");

            // 403 — only the request owner can scan the pickup QR
            if (bloodRequest.RequestedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to scan the pickup QR for this request.");

            // Mark token as used
            token.IsUsed = true;
            _uow.QrTokens.Update(token);

            // Mark request as Completed
            bloodRequest.Status = BloodRequestStatus.Completed;
            bloodRequest.IsBloodReceived = true;
            _uow.BloodRequests.Update(bloodRequest);

            await _uow.SaveChangesAsync();

            return new PickupScanResponseDto
            {
                Message = "Blood received successfully",
                RequestId = requestId,
                Status = BloodRequestStatus.Completed.ToString(),
            };
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
