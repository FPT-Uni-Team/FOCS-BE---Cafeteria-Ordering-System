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

            CreateMap<MenuItem, MenuItemDetailAdminDTO>().ReverseMap();
            CreateMap<MenuItemVariant, MenuItemVariantDTO>().ReverseMap();
            CreateMap<VariantGroup, VariantGroupDTO>().ReverseMap();
            // Admin Mappings for Menu item
            CreateMap<MenuItem, MenuItemAdminDTO>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            // Admin Mappings for Brand
            CreateMap<Brand, BrandAdminDTO>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            // Admin Mappings for Store
            CreateMap<Store, StoreAdminDTO>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            // Admin Mappings for Store
            CreateMap<Coupon, CouponAdminDTO>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Brand, CreateAdminBrandRequest>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<StoreSetting, StoreSettingDTO>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Promotion, PromotionDTO>().ReverseMap();

            CreateMap<PromotionItemCondition, PromotionItemConditionDTO>().ReverseMap();

            CreateMap<MenuCategory, CreateCategoryRequest>().ReverseMap();
            CreateMap<MenuCategory, UpdateCategoryRequest>().ReverseMap();

            CreateMap<UserStore, UserStoreDTO>().ReverseMap();

        }
    }
}
