using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IQrService
    {
        // User generates QR for their donation
        Task<QrTokenResponseDto> GenerateDonationQrAsync(int donationId, string userId);

        // Hospital Admin generates pickup QR for a request
        Task<QrTokenResponseDto> GeneratePickupQrAsync(int requestId, string userId);

        // HospitalAdmin generates withdrawal QR for a general donation (no BloodRequest)
        Task<QrTokenResponseDto> GenerateGeneralDonationPickupQrAsync(int donationId, string hospitalAdminId);

        // Hospital scans donation QR → confirms receiving blood from donor
        Task<DonationScanResponseDto> ScanDonationQrAsync(string qrToken, string hospitalAdminId);

        // User or HospitalAdmin scans pickup QR
         // Case 1 (BloodRequest) → Completed | Case 2 (General Donation) → Withdrawn
       Task<PickupScanResponseDto> ScanPickupQrAsync(string qrToken, string userId, string userRole);

        // User generates QR token for a redeemed reward
          Task<RewardQrResponseDto> GenerateRewardQrAsync(int userRewardId, string userId);
    
        // Hospital Admin scans reward QR → marks redemption as Used
          Task<RewardScanResponseDto> ScanRewardQrAsync(string qrToken);
    }
}
