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

        // Hospital scans donation QR → confirms receiving blood from donor
        Task<DonationScanResponseDto> ScanDonationQrAsync(int donationId, string qrToken, string hospitalAdminId);

        // User (patient) scans pickup QR → confirms receiving blood bag
        Task<PickupScanResponseDto> ScanPickupQrAsync(int requestId, string qrToken, string userId);
    }
}
