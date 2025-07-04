﻿using AutoMapper;
using FOCS.Application.DTOs;
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
            CreateMap<Category, MenuCategoryDTO>().ReverseMap();
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
            // Mappings for Table
            CreateMap<Table, TableDTO>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Brand, CreateAdminBrandRequest>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<StoreSetting, StoreSettingDTO>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<PromotionDTO, Promotion>().ReverseMap()
                .ForMember(dest => dest.AcceptForItems, 
                    opt => opt.MapFrom(src => src.AcceptForItems != null ?
                                           src.AcceptForItems.Distinct().ToList() : null))
                .ForMember(dest => dest.CouponIds, 
                    opt => opt.MapFrom(src => src.Coupons != null ? 
                                            src.Coupons.Select(c => c.Id) : null))
                .ForMember(dest => dest.PromotionItemConditionDTO,
                    opt => opt.MapFrom(src => src.PromotionItemConditions.FirstOrDefault()));

            CreateMap<PromotionItemCondition, PromotionItemConditionDTO>().ReverseMap();

            CreateMap<Category, CreateCategoryRequest>().ReverseMap();
            CreateMap<Category, UpdateCategoryRequest>().ReverseMap();

            CreateMap<UserStore, UserStoreDTO>().ReverseMap();

            CreateMap<User, UserProfileDTO>().ReverseMap();


            // Order mapping
            CreateMap<OrderDTO, Order.Infrastucture.Entities.Order>().ReverseMap(); 
            CreateMap<OrderDetailDTO, OrderDetail>().ReverseMap();

            CreateMap<VariantOptionDTO, MenuItemVariant>().ReverseMap();

            CreateMap<MenuItemVariantGroup, CreateMenuItemVariantGroupRequest>().ReverseMap();

            CreateMap<VariantGroup, CreateVariantGroupRequest>().ReverseMap();
        }
    }
}
