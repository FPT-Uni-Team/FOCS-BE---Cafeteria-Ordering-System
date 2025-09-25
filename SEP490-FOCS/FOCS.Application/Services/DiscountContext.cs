using FOCS.Application.Services.ApplyStrategy;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class DiscountContext
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IStoreSettingService _storeSettingService;
        private readonly IPromotionService _promotionService;

        private readonly IRepository<SystemConfiguration> _systemConfig;

        private readonly UserManager<User> _userManager;
        public DiscountContext(IServiceProvider serviceProvider, IStoreSettingService storeSettingService, IRepository<SystemConfiguration> systemConfig, IPromotionService promotionService, UserManager<User> userManager)
        {
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _storeSettingService = storeSettingService;
            _promotionService = promotionService;
            _systemConfig = systemConfig;
        }

        public async Task<DiscountResultDTO> CalculateDiscountAsync(ApplyDiscountOrderRequest applyDiscountOrderRequest, string? couponCode, DiscountStrategy discountStrategy, string userId)
        {
            var finalDiscountResult = new DiscountResultDTO();

            IDiscountStrategyService strategyInstance = discountStrategy switch
            {
                DiscountStrategy.CouponOnly => _serviceProvider.GetRequiredService<CouponOnlyStrategy>(),
                DiscountStrategy.PromotionOnly => _serviceProvider.GetRequiredService<PromotionOnlyStrategy>(),
                DiscountStrategy.CouponThenPromotion => _serviceProvider.GetRequiredService<CouponThenPromotionStrategy>(),
                DiscountStrategy.MaxDiscountOnly => _serviceProvider.GetRequiredService<MaxDiscountOnlyStrategy>(),
                _ => throw new NotImplementedException()
            };

            var storeSetting = await _storeSettingService.GetStoreSettingAsync(applyDiscountOrderRequest.StoreId);

            finalDiscountResult = await strategyInstance.ApplyDiscountAsync(applyDiscountOrderRequest, couponCode);

            if (storeSetting.AllowCombinePromotionAndCoupon)
            {
                finalDiscountResult = await _promotionService.ApplyEligiblePromotions(finalDiscountResult);
            }

            if(applyDiscountOrderRequest.IsUseLoyatyPoint.HasValue && applyDiscountOrderRequest.IsUseLoyatyPoint == true)
            {
                if(applyDiscountOrderRequest.Point.HasValue && applyDiscountOrderRequest.Point > 0)
                {
                    var user = await _userManager.FindByIdAsync(userId);

                    //if guest
                    if (user != null)
                    {
                        if (user!.FOCSPoint < applyDiscountOrderRequest.Point)
                        {
                            applyDiscountOrderRequest.Point = user!.FOCSPoint;
                        }

                        finalDiscountResult.IsUsePoint = true;
                        finalDiscountResult.Point = applyDiscountOrderRequest.Point;

                        var spendingRate = (await _storeSettingService.GetStoreSettingAsync(applyDiscountOrderRequest.StoreId))!.SpendingRate ?? 0;

                        var discountAmountBasedOnPoint = ((decimal)applyDiscountOrderRequest.Point * (decimal)spendingRate) * 1000;

                        if (discountAmountBasedOnPoint > finalDiscountResult.SubTotal - finalDiscountResult.TotalDiscount)
                        {
                            discountAmountBasedOnPoint = finalDiscountResult.SubTotal - finalDiscountResult.TotalDiscount;
                            finalDiscountResult.Point = (int)(discountAmountBasedOnPoint / (spendingRate * 1000));
                        }

                        finalDiscountResult.TotalDiscount += discountAmountBasedOnPoint;
                        finalDiscountResult.TotalPrice = finalDiscountResult.TotalPrice -= discountAmountBasedOnPoint < 0 ? 0 : finalDiscountResult.TotalPrice -= discountAmountBasedOnPoint;
                    } else
                    {
                        finalDiscountResult.IsUsePoint = false;
                        finalDiscountResult.Point = 0;
                    }
                }
            }

            return finalDiscountResult;
        }
    }
}
