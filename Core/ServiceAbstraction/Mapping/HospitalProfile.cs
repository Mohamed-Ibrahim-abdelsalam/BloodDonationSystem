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
    public class HospitalProfile : Profile
    {
        public HospitalProfile()
        {
            // Hospital → HospitalDto (POST / PUT response)
            CreateMap<Hospital, HospitalDto>()
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.Phone));

            // Hospital → HospitalListItemDto (admin dashboard list)
            CreateMap<Hospital, HospitalListItemDto>()
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.Phone));

            // Hospital → HospitalDropdownItemDto (public dropdown)
            CreateMap<Hospital, HospitalDropdownItemDto>();
        }
    }
}
