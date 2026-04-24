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
        Task<DonationResponseDto> CreateAsync(CreateDonationDto dto, string userId);
        Task<IEnumerable<MyDonationDto>> GetMyDonationsAsync(string userId);
        Task CancelAsync(int donationId, string userId);
    }
}
