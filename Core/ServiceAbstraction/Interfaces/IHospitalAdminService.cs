using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IHospitalAdminService
    {

        Task<HospitalAdminDto> CreateAsync(CreateHospitalAdminDto dto);
        Task<HospitalAdminsDashboardDto> GetAllAsync();
        Task<HospitalAdminDetailDto> GetByIdAsync(string id);
        Task<HospitalAdminDto> UpdateAsync(string id, UpdateHospitalAdminDto dto);
        Task DeleteAsync(string id);
    }
}
