using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IHospitalService
    {
       
        Task<HospitalDto> CreateAsync(CreateHospitalDto dto);
        Task<HospitalsDashboardDto> GetAllAsync();
        Task<HospitalDto> UpdateAsync(int id, UpdateHospitalDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<HospitalDropdownItemDto>> GetDropdownAsync();
    }
}
