using ServiceAbstraction.Dtos.BloodRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IBloodRequestService
    {
        Task<BloodRequestDto> CreateAsync(CreateBloodRequestDto dto, string userId);
        Task<IEnumerable<BloodRequestDto>> GetAllAsync(BloodRequestQueryParams queryParams);
        Task<BloodRequestDetailDto> GetByIdAsync(int id);
        Task<IEnumerable<MyBloodRequestDto>> GetMyRequestsAsync(string userId);
        Task DeleteAsync(int id, string userId);
    }
}
