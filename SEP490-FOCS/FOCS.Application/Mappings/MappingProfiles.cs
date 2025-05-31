using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;

namespace FOCS.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<UserRefreshToken, UserRefreshTokenDTO>()
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.Token));

            CreateMap<UserRefreshTokenDTO, UserRefreshToken>()
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.RefreshToken))
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            // Menu Mappings
            CreateMap<MenuCategory, MenuCategoryDTO>().ReverseMap();
            CreateMap<MenuItem, MenuItemDTO>().ReverseMap();
            CreateMap<MenuItemVariant, MenuItemVariantDTO>().ReverseMap();
            CreateMap<VariantGroup, VariantGroupDTO>().ReverseMap();
            // Admin Mappings
            CreateMap<MenuItem, MenuItemAdminServiceDTO>().ReverseMap();
        }
    }
}
