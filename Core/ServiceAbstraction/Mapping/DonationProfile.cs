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
            // CreateDonationDto → Donation entity
            CreateMap<CreateDonationDto, Donation>()
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.DonorUserId, o => o.Ignore());

            // Donation → DonationResponseDto
            CreateMap<Donation, DonationResponseDto>()
                .ForMember(d => d.DonorData, o => o.MapFrom(s => new DonorDataDto
                {
                    Age = s.Age,
                    Weight = s.Weight,
                    HasTattoo = s.HasTattoo,
                    LastDonationDate = s.LastDonationDate,
                    Address = s.Address,
                    MedicalCondition = s.MedicalCondition,
                }));

            // Donation → MyDonationDto
            CreateMap<Donation, MyDonationDto>()
                .ForMember(d => d.HospitalName, o => o.MapFrom(s =>
                    s.BloodRequest != null
                        ? s.BloodRequest.HospitalName
                        : null));
        }
    }
}
