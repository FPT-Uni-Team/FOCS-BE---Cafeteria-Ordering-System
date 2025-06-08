using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class AdminCouponService : IAdminCouponService
    {
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IMapper _mapper;

        public AdminCouponService(IRepository<Coupon> couponRepository, IRepository<Promotion> promotionRepository, IMapper mapper)
        {
            _couponRepository = couponRepository;
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<CouponAdminServiceDTO> CreateCouponAsync(CouponAdminServiceDTO dto, string userId)
        {
            // Code empty => generate unique code
            string couponCode = string.IsNullOrWhiteSpace(dto.Code)
                ? await GenerateUniqueCouponCodeAsync()
                : dto.Code.Trim();

            // Check unique code
            var existing = await _couponRepository.AsQueryable()
                                                  .AnyAsync(c => c.Code == couponCode && !c.IsDeleted);
            if (existing)
                throw new InvalidOperationException("Coupon code already exists.");

            // Check dates
            if (dto.StartDate > dto.EndDate)
                throw new ArgumentException("Start date must be before end date.");

            // Map DTO to entity
            var newCoupon = _mapper.Map<Coupon>(dto);
            newCoupon.Id = Guid.NewGuid();
            newCoupon.Code = couponCode;
            newCoupon.IsDeleted = false;
            newCoupon.CreatedAt = DateTime.UtcNow;
            newCoupon.CreatedBy = userId;

            await _couponRepository.AddAsync(newCoupon);
            await _couponRepository.SaveChangesAsync();

            return _mapper.Map<CouponAdminServiceDTO>(newCoupon);
        }

        private async Task<string> GenerateUniqueCouponCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            string newCode;
            bool exists;

            do
            {
                newCode = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                exists = await _couponRepository.AsQueryable()
                    .AnyAsync(c => c.Code == newCode && !c.IsDeleted);

            } while (exists);

            return newCode;
        }

        public async Task<PagedResult<CouponAdminServiceDTO>> GetAllCouponsAsync(UrlQueryParameters query)
        {
            var couponQuery = _couponRepository.AsQueryable().Where(c => !c.IsDeleted);

            // Search
            if (!string.IsNullOrEmpty(query.SearchBy) && !string.IsNullOrEmpty(query.SearchValue))
            {
                var value = query.SearchValue.ToLower();
                switch (query.SearchBy.ToLower())
                {
                    case "code":
                        couponQuery = couponQuery.Where(c => c.Code.ToLower().Contains(value));
                        break;
                    case "description":
                        couponQuery = couponQuery.Where(c => c.Description.ToLower().Contains(value));
                        break;
                    case "discounttype":
                        if (Enum.TryParse<DiscountType>(query.SearchValue, true, out var type))
                            couponQuery = couponQuery.Where(c => c.DiscountType == type);
                        break;
                }
            }

            // Sort
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                bool desc = query.SortOrder?.ToLower() == "desc";
                couponQuery = query.SortBy.ToLower() switch
                {
                    "code" => desc ? couponQuery.OrderByDescending(c => c.Code) : couponQuery.OrderBy(c => c.Code),
                    "value" => desc ? couponQuery.OrderByDescending(c => c.Value) : couponQuery.OrderBy(c => c.Value),
                    "startdate" => desc ? couponQuery.OrderByDescending(c => c.StartDate) : couponQuery.OrderBy(c => c.StartDate),
                    "enddate" => desc ? couponQuery.OrderByDescending(c => c.EndDate) : couponQuery.OrderBy(c => c.EndDate),
                    "isactive" => desc ? couponQuery.OrderByDescending(c => c.IsActive) : couponQuery.OrderBy(c => c.IsActive),
                    _ => couponQuery
                };
            }

            // Pagination
            var total = await couponQuery.CountAsync();
            var items = await couponQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<CouponAdminServiceDTO>>(items);
            return new PagedResult<CouponAdminServiceDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<bool> UpdateCouponAsync(Guid id, CouponAdminServiceDTO dto, string userId)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null || coupon.IsDeleted)
                return false;

            // Check unique code (exclude current coupon)
            var existing = await _couponRepository.AsQueryable()
                                                  .AnyAsync(c => c.Id != id && c.Code == dto.Code && !c.IsDeleted);
            if (existing)
                throw new InvalidOperationException("Coupon code already exists.");

            // Check dates
            if (dto.StartDate > dto.EndDate)
                throw new ArgumentException("Start date must be before end date.");

            _mapper.Map(dto, coupon);
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedBy = userId;

            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCouponAsync(Guid id, string userId)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null || coupon.IsDeleted)
                return false;

            coupon.IsDeleted = true;
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedBy = userId;

            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> TrackCouponUsageAsync(Guid couponId)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            if (coupon == null || coupon.IsDeleted || !coupon.IsActive)
                return -1;

            if (coupon.CountUsed >= coupon.MaxUsage)
                return -1;

            return coupon.MaxUsage - coupon.CountUsed;
        }

        public async Task<bool> SetCouponStatusAsync(Guid couponId, bool isActive, string userId)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            if (coupon == null || coupon.IsDeleted)
                return false;

            coupon.IsActive = isActive;
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedBy = userId;

            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignCouponToPromotionAsync(Guid couponId, Guid promotionId, string userId)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            if (coupon == null || coupon.IsDeleted)
                return false;

            var promotion = await _promotionRepository.GetByIdAsync(promotionId);
            if (promotion == null || promotion.IsDeleted)
                return false;

            // Check if coupon dates are within promotion dates
            if (coupon.StartDate < promotion.StartDate || coupon.EndDate > promotion.EndDate)
                return false;

            // Update coupon
            coupon.PromotionId = promotionId;
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedBy = userId;

            await _couponRepository.SaveChangesAsync();
            return true;
        }

    }
}
