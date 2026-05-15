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
            // ── Shared formatting helpers ─────────────────────────────────────
            // BloodType enum → "O+" style string is done inside the service
            // (via BloodRequestService.FormatBloodType) so mapping stays simple.

            // ── BloodRequest → BloodRequestDto (POST response + GET list) ─────
            CreateMap<BloodRequest, BloodRequestDto>()
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "+")))
                .ForMember(d => d.Priority,
                    o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.HospitalName,
                    o => o.MapFrom(s => s.Hospital != null
                        ? s.Hospital.Name
                        : s.HospitalName));

            // ── BloodRequest → BloodRequestDetailDto (GET by id) ──────────────
            CreateMap<BloodRequest, BloodRequestDetailDto>()
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "+")))
                .ForMember(d => d.Priority,
                    o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.HospitalName,
                    o => o.MapFrom(s => s.Hospital != null
                        ? s.Hospital.Name
                        : s.HospitalName))
                .ForMember(d => d.CreatedBy,
                    o => o.MapFrom(s => s.RequestedByUser != null
                        ? s.RequestedByUser.FullName
                        : string.Empty));

            // ── BloodRequest → MyBloodRequestDto (GET /my) ────────────────────
            CreateMap<BloodRequest, MyBloodRequestDto>()
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "+")))
                .ForMember(d => d.Priority,
                    o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.HospitalName,
                    o => o.MapFrom(s => s.Hospital != null
                        ? s.Hospital.Name
                        : s.HospitalName));
        }
    }
}
