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
    public class RewardProfile : Profile
    {
        public RewardProfile()
        {
            // Reward → RewardDto (catalog list)
            CreateMap<Reward, RewardDto>();

            // Reward → RewardDetailDto
            CreateMap<Reward, RewardDetailDto>();

            // UserReward → UserRewardDto
            CreateMap<UserReward, UserRewardDto>()
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Reward.Title))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}
