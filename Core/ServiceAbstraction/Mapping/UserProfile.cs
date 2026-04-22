using AutoMapper;
using BloodDonationSystem.Models;
using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // ApplicationUser → UserProfileDto
            CreateMap<ApplicationUser, UserProfileDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email));

            // ApplicationUser → UpdateProfileResponseDto
            CreateMap<ApplicationUser, UpdateProfileResponseDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.Message, o => o.Ignore());
        }
    }
}
