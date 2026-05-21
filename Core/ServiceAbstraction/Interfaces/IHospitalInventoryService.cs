using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IHospitalInventoryService
    {
        Task<HospitalInventoryResponseDto> GetInventoryAsync(string userId);
    }
}
