using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly IRepository<OrderEntity> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;

        private readonly IMapper _mapper;
        private readonly ILogger<CashierService> _logger;

        public CashierService(IRepository<OrderEntity> orderRepository, IRepository<OrderDetail> orderDetailRepository, IMapper mapper, ILogger<CashierService> logger)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<OrderDTO>> GetOrders(UrlQueryParameters query, string storeId)
        {
            var orderQueries = _orderRepository.AsQueryable().Where(x => x.StoreId == Guid.Parse(storeId));

            orderQueries = ApplyFilters(orderQueries, query);
            orderQueries = ApplySearch(orderQueries, query);
            orderQueries = ApplySort(orderQueries, query);

            var total = await orderQueries.CountAsync();
            var items = await orderQueries
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<OrderDTO>>(items);
            return new PagedResult<OrderDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<bool> UpdatePaymentStatus(Guid orderId, UpdatePaymentStatusRequest updatePaymentStatus, string storeId)
        {
            try
            {
                var order = await _orderRepository.AsQueryable().FirstOrDefaultAsync(x => x.StoreId == Guid.Parse(storeId) && x.Id == orderId);

                ConditionCheck.CheckCondition(order != null, Errors.Common.NotFound);

                order!.PaymentStatus = updatePaymentStatus.PaymentStatus;
                order.UpdatedAt = DateTime.UtcNow;

                _orderRepository.Update(order);
                await _orderRepository.SaveChangesAsync();

                return true;
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }


        #region private methods

        private static IQueryable<OrderEntity> ApplyFilters(IQueryable<OrderEntity> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                query = key.ToLowerInvariant() switch
                {
                    "order_status" when Enum.TryParse<OrderStatus>(value, true, out var orderStatus) =>
                        query.Where(p => p.OrderStatus == orderStatus),
                    //"period_date" when 
                    "payment_status" when Enum.TryParse<PaymentStatus>(value, true, out var paymentStatus) =>
                        query.Where(p => p.PaymentStatus == paymentStatus),
                    _ => query
                };
            }

            return query;
        }

        private static IQueryable<OrderEntity> ApplySearch(IQueryable<OrderEntity> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                "order_code" => query.Where(p => p.OrderCode.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<OrderEntity> ApplySort(IQueryable<OrderEntity> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query;

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
                "create_at" => isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                "order_status" => isDescending
                    ? query.OrderByDescending(p => p.OrderStatus)
                    : query.OrderBy(p => p.OrderStatus),
                "payment_status" => isDescending
                    ? query.OrderByDescending(p => p.PaymentStatus)
                    : query.OrderBy(p => p.PaymentStatus),
                _ => query
            };
        }

        #endregion 
    }
}
