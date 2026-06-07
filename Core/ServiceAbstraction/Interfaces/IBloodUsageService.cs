using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IBloodUsageService
    {
      
        Task<BloodUsageResponseDto> GetBloodUsageAsync(string userId, string period);
    }
}
