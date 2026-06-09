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
    public class DonationProfile : Profile
    {
        public DonationProfile()
        {
            // ── Donation → DonationResponseDto (POST response) ────────────────
            CreateMap<Donation, DonationResponseDto>()
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "")))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.HospitalId,
                    o => o.MapFrom(s => s.HospitalId))
                .ForMember(d => d.HospitalName,
                    o => o.MapFrom(s => s.Hospital != null ? s.Hospital.Name : string.Empty))
                .ForMember(d => d.DonorData,
                    o => o.MapFrom(s => new DonorDataDto
                    {
                        Age = s.Age,
                        Weight = s.Weight,
                        HasTattoo = s.HasTattoo,
                        LastDonationDate = s.LastDonationDate,
                        // Donation.MedicalCondition is stored as string ("True"/"False")
                        // because the DB column pre-dates the bool DTO.
                        // Parse it back to bool for the response.
                        MedicalCondition = s.MedicalCondition == "True",
                    }));

            // ── Donation → MyDonationDto (GET /my list) ───────────────────────
            CreateMap<Donation, MyDonationDto>()
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "")))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.HospitalId,
                    o => o.MapFrom(s => s.HospitalId))
                .ForMember(d => d.HospitalName,
                    o => o.MapFrom(s => s.Hospital != null ? s.Hospital.Name : string.Empty));
        }
    }
}
