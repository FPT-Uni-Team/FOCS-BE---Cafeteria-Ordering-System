using AutoMapper;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() {
            CreateMap<UserRefreshToken, UserRefreshTokenDTO>()
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.Token));

            CreateMap<UserRefreshTokenDTO, UserRefreshToken>()
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.User, opt => opt.Ignore()) 
                .ForMember(dest => dest.Id, opt => opt.Ignore()); 
        }
    }
}
