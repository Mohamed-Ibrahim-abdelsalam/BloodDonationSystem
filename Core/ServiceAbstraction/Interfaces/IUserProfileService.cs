using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task<UpdateProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    }
}
