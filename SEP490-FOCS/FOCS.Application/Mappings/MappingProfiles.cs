using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using OrderEnity = FOCS.Order.Infrastucture.Entities.Order;

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

            CreateMap<MenuItem, MenuItemDetailAdminDTO>()
                 .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                     src.Images
                         .Select(i => new UploadedImageResult
                         {
                             IsMain = i.IsMain,
                             Url = i.Url
                         })
                 ))
                 .ReverseMap()
                 .ForMember(dest => dest.Images, opt => opt.Ignore());

            CreateMap<Feedback, CreateFeedbackRequest>()
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ActorId));

            CreateMap<Feedback, FeedbackDTO>().ReverseMap();

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
                .ForMember(dest => dest.QrCode, opt => opt.Ignore())
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

            CreateMap<User, StaffProfileDTO>().ReverseMap();

            // Order mapping
            // Order -> OrderDTO
            CreateMap<OrderEnity, OrderDTO>()
                .ForMember(dest => dest.OrderCode,
                           opt => opt.MapFrom(src => src.OrderCode.ToString()))
                .ForMember(dest => dest.CouponCode,
                           opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.Code : null))
                .ForMember(dest => dest.OrderDetails,
                           opt => opt.MapFrom(src => src.OrderDetails))
                // ignore các navigation không có trong DTO
                .ForSourceMember(src => src.Store, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.Table, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.OrderWrap, opt => opt.DoNotValidate());

            // OrderDTO -> Order
            CreateMap<OrderDTO, OrderEnity>()
                .ForMember(dest => dest.OrderCode,
                           opt => opt.MapFrom(src =>
                               !string.IsNullOrEmpty(src.OrderCode) ? Convert.ToInt64(src.OrderCode) : 0))
                .ForMember(dest => dest.Coupon, opt => opt.Ignore()) // sẽ lấy từ DB
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.Table, opt => opt.Ignore())
                .ForMember(dest => dest.OrderWrap, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDetails,
                           opt => opt.MapFrom(src => src.OrderDetails));

            // OrderDetail
            CreateMap<OrderDetail, OrderDetailDTO>()
                .ForMember(dest => dest.MenuItemName,
                           opt => opt.MapFrom(src => src.MenuItem != null ? src.MenuItem.Name : string.Empty))
                .ForMember(dest => dest.Variants,
                           opt => opt.MapFrom(src => src.Variants));

            CreateMap<OrderDetailDTO, OrderDetail>()
                .ForMember(dest => dest.MenuItem, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());

            // MenuItemVariant
            CreateMap<MenuItemVariant, OrderDetailVariantDTO>()
                .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VariantName, opt => opt.MapFrom(src => src.Name));

            CreateMap<OrderDetailVariantDTO, MenuItemVariant>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.VariantId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.VariantName));



            CreateMap<VariantOptionDTO, MenuItemVariant>().ReverseMap();

            CreateMap<MenuItemVariantGroup, CreateMenuItemVariantGroupRequest>().ReverseMap();

            CreateMap<VariantGroup, CreateVariantGroupRequest>().ReverseMap();

            CreateMap<VariantGroup, VariantGroupResponse>().ReverseMap();

            CreateMap<StoreAdminResponse, StoreSetting>().ReverseMap()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Store.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Store.Address))
                .ForMember(dest => dest.CustomTaxRate, opt => opt.MapFrom(src => src.Store.CustomTaxRate));
        }
    }
}
