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
    public class AdminProfile : Profile
    {
        public AdminProfile()
        {
            // ── BloodRequest → AdminRequestItemDto ────────────────────────────
            CreateMap<BloodRequest, AdminRequestItemDto>()
                .ForMember(d => d.UserId,
                    o => o.MapFrom(s => s.RequestedByUserId))
                .ForMember(d => d.PatientName,
                    o => o.MapFrom(s => s.RequestedByUser != null
                        ? s.RequestedByUser.FullName
                        : string.Empty))
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "+")))
                .ForMember(d => d.Priority,
                    o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()));

            // ── Donation → AdminDonationItemDto ───────────────────────────────
            CreateMap<Donation, AdminDonationItemDto>()
                .ForMember(d => d.UserId,
                    o => o.MapFrom(s => s.DonorUserId))
                .ForMember(d => d.DonorName,
                    o => o.MapFrom(s => s.DonorUser != null
                        ? s.DonorUser.FullName
                        : string.Empty))
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "+")))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Quantity,
                    o => o.MapFrom(s => s.BloodRequest != null
                        ? s.BloodRequest.Quantity
                        : 1));

            // ── ApplicationUser → AdminUserListItemDto ────────────────────────
            // NOTE: LastDonation and Status are computed values — set after mapping
            // via ResolveUserListItem() in AdminService, not here.
            // AutoMapper handles the flat fields; service handles computed ones.
            CreateMap<ApplicationUser, AdminUserListItemDto>()
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "+")))
                .ForMember(d => d.LastDonation,
                    o => o.Ignore())   // computed from Donations collection in service
                .ForMember(d => d.Status,
                    o => o.Ignore());  // computed from LastDonation in service

            // ── ApplicationUser → AdminUserDetailDto ──────────────────────────
            CreateMap<ApplicationUser, AdminUserDetailDto>()
                .ForMember(d => d.BloodType,
                    o => o.MapFrom(s => s.BloodType.ToString().Replace("_", "+")))
                .ForMember(d => d.Gender,
                    o => o.MapFrom(s => s.Gender.ToString()))
                .ForMember(d => d.Role,
                    o => o.MapFrom(s => s.Role.ToString()))
                .ForMember(d => d.TotalDonations,
                    o => o.MapFrom(s => s.Donations != null ? s.Donations.Count : 0))
                .ForMember(d => d.LastDonation,
                    o => o.Ignore())  // computed in service
                .ForMember(d => d.Status,
                    o => o.Ignore()); // computed in service
        }
    }
}
