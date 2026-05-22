using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IRewardAdminService
    {
        Task<AdminRewardDto> CreateAsync(CreateRewardDto dto);
        Task<AdminRewardDto> UpdateAsync(int id, UpdateRewardDto dto);
        Task DeleteAsync(int id);
    }
}
