using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FOCS.Application.Services
{
    public class MenuItemVariantService : IMenuItemVariantService
    {
        private readonly IRepository<MenuItemVariant> _menuItemVariantRepository;
        private readonly IMapper _mapper;
        public MenuItemVariantService(IRepository<MenuItemVariant> menuItemVariantRepository, IMapper mapper)
        {
            _menuItemVariantRepository = menuItemVariantRepository;
            _mapper = mapper;
        }

        public async Task<MenuItemVariantDTO> CreateMenuItemVariant(MenuItemVariantDTO request, Guid storeId)
        {
            var isExist = await _menuItemVariantRepository.AsQueryable().AnyAsync(x => x.Name == request.Name && x.CreatedBy == storeId.ToString());
            ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist);

            var mapper = _mapper.Map<MenuItemVariant>(request);
            
            mapper.CreatedBy = storeId.ToString();
            
            await _menuItemVariantRepository.AddAsync(mapper);
            await _menuItemVariantRepository.SaveChangesAsync();

            return _mapper.Map<MenuItemVariantDTO>(mapper);
        }

        public async Task<List<VariantDTO>> ListVariantByStore(string storeId)
        {
            var variants = await _menuItemVariantRepository.AsQueryable().Where(x => x.CreatedBy == storeId).ToListAsync();

            return variants.Select(x => new VariantDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price
            }).ToList();
        }

        public async Task<List<MenuItemVariantDTO>> ListVariantsWithIds(List<Guid> ids, Guid storeId)
        {
            var variants = await _menuItemVariantRepository.AsQueryable()
                                                            .AsNoTracking()
                                                            .Where(x => ids.Contains(x.Id) && x.CreatedBy == storeId.ToString() && !x.IsDeleted && x.IsAvailable)
                                                            .ToListAsync();

            return _mapper.Map<List<MenuItemVariantDTO>>(variants);
        }

        public async Task<bool> AssignVariantGroupToVariants(List<Guid> ids, Guid variantGroupId)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException("List of variant IDs cannot be null or empty.");

            try
            {
                var variants = await _menuItemVariantRepository
                    .AsQueryable()
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync();

                if (variants.Count != ids.Count)
                    throw new Exception("Some variant IDs were not found.");

                foreach (var variant in variants)
                {
                    variant.VariantGroupId = variantGroupId;
                }

                _menuItemVariantRepository.UpdateRange(variants);
                await _menuItemVariantRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<MenuItemVariantDTO> GetVariantDetail(Guid id, Guid storeId)
        {
            var isExist = await _menuItemVariantRepository.GetByIdAsync(id);

            ConditionCheck.CheckCondition(isExist != null, Errors.Common.NotFound);

            return _mapper.Map<MenuItemVariantDTO>(isExist);
        }

        public async Task<PagedResult<MenuItemVariantDTO>> ListVariants(UrlQueryParameters query, Guid storeId)
        {
            var variantQuery = _menuItemVariantRepository.AsQueryable()
                .Where(p => p.CreatedBy == storeId.ToString() && !p.IsDeleted);

            variantQuery = ApplyFilters(variantQuery, query);
            variantQuery = ApplySearch(variantQuery, query);
            variantQuery = ApplySort(variantQuery, query);

            var total = await variantQuery.CountAsync();
            var items = await variantQuery
                .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

            var mapped = _mapper.Map<List<MenuItemVariantDTO>>(items);
            return new PagedResult<MenuItemVariantDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<bool> RemoveMenuItemVariant(Guid id, Guid storeId)
        {
            try
            {
                var isExist = await _menuItemVariantRepository.GetByIdAsync(id);

                ConditionCheck.CheckCondition(isExist != null, Errors.Common.NotFound);

                _menuItemVariantRepository.Remove(isExist!);
                await _menuItemVariantRepository.SaveChangesAsync();
            } catch(Exception ex) { return false; }
            return true;
        }

        public async Task<bool> UpdateMenuItemVariant(Guid Id, MenuItemVariantDTO request, Guid storeId)
        {
            try
            {
                var isExist = await _menuItemVariantRepository.GetByIdAsync(Id);
                ConditionCheck.CheckCondition(isExist != null, Errors.Common.NotFound);

                _mapper.Map(request, isExist);

                isExist!.Id = Id;
                isExist!.CreatedBy = storeId.ToString();
                isExist.UpdatedAt = DateTime.UtcNow;
                isExist.UpdatedBy = storeId.ToString();

                _menuItemVariantRepository.Update(isExist);
                await _menuItemVariantRepository.SaveChangesAsync();
            } catch (Exception ex) { return false; }
            return true;
        }

        private static IQueryable<MenuItemVariant> ApplyFilters(IQueryable<MenuItemVariant> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            //foreach (var (key, value) in parameters.Filters)
            //{
            //    query = key.ToLowerInvariant() switch
            //    {
            //        "promotion_type" when Enum.TryParse<PromotionType>(value, true, out var promotionType) =>
            //            query.Where(p => p.PromotionType == promotionType),
            //        "start_date" => query.Where(p => p.StartDate >= DateTime.Parse(value)),
            //        "end_date" => query.Where(p => p.EndDate <= DateTime.Parse(value)),
            //        "status" when Enum.TryParse<PromotionStatus>(value, true, out var status) =>
            //            status switch
            //            {
            //                PromotionStatus.Incomming => query.Where(p => p.StartDate > DateTime.UtcNow),
            //                PromotionStatus.OnGoing => query.Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow),
            //                PromotionStatus.Expired => query.Where(p => p.EndDate < DateTime.UtcNow),
            //                PromotionStatus.UnAvailable => query.Where(p => p.IsActive == false),
            //                _ => query
            //            },
            //        _ => query
            //    };
            //}

            return query;
        }

        private static IQueryable<MenuItemVariant> ApplySearch(IQueryable<MenuItemVariant> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                "name" => query.Where(p => p.Name.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<MenuItemVariant> ApplySort(IQueryable<MenuItemVariant> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query;

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
                "name" => isDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name)
            };
        }
    }
}
