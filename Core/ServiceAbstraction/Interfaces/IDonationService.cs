using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IDonationService
    {
        /// <summary>
        /// POST /api/donations — creates a request-based or general donation.
        /// BloodType is taken automatically from the authenticated user's profile.
        /// </summary>
        Task<DonationResponseDto> CreateAsync(CreateDonationDto dto, string userId);

        /// <summary>GET /api/donations/my — all donations for the authenticated user.</summary>
        Task<IEnumerable<MyDonationDto>> GetMyDonationsAsync(string userId);

        /// <summary>POST /api/donations/{id}/cancel — cancel a pending donation.</summary>
        Task CancelAsync(int donationId, string userId);
    }
}
