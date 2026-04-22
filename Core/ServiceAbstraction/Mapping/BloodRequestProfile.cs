using AutoMapper;
using BloodDonationSystem.Models;
using ServiceAbstraction.Dtos.BloodRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServiceAbstraction.Mapping
{
    public class BloodRequestProfile : Profile
    {
        public BloodRequestProfile()
        {
            // Entity → BloodRequestDto (list + POST response)
            CreateMap<BloodRequest, BloodRequestDto>()
                .ForMember(d => d.CreatedBy,
                    o => o.MapFrom(s => s.RequestedByUser != null
                        ? s.RequestedByUser.FullName
                        : string.Empty));

            // Entity → BloodRequestDetailDto (GET by id)
            CreateMap<BloodRequest, BloodRequestDetailDto>()
                .ForMember(d => d.CreatedBy,
                    o => o.MapFrom(s => s.RequestedByUser != null
                        ? s.RequestedByUser.FullName
                        : string.Empty));

            // Entity → MyBloodRequestDto (GET /my)
            CreateMap<BloodRequest, MyBloodRequestDto>();

            // CreateBloodRequestDto → Entity
            CreateMap<CreateBloodRequestDto, BloodRequest>()
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.RequestedByUserId, o => o.Ignore());
        }
    }
}
