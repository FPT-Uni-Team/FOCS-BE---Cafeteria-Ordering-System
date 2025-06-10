using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class AdminBrandService : IAdminBrandService
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IMapper _mapper;

        public AdminBrandService(IRepository<Brand> brandRepository, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        public async Task<BrandAdminDTO> CreateBrandAsync(CreateAdminBrandRequest dto, string userId)
        {
            CheckValidInput(userId);
            var newBrand = _mapper.Map<Brand>(dto);
            newBrand.Id = Guid.NewGuid();
            newBrand.IsDelete = false;
            newBrand.CreatedAt = DateTime.UtcNow;
            newBrand.CreatedBy = userId;

            await _brandRepository.AddAsync(newBrand);
            await _brandRepository.SaveChangesAsync();

            return _mapper.Map<BrandAdminDTO>(newBrand);
        }

        public async Task<PagedResult<BrandAdminDTO>> GetAllBrandsAsync(UrlQueryParameters query, string userId)
        {
            CheckValidInput(userId);
            var brandQuery = _brandRepository.AsQueryable().Where(b => !b.IsDelete && b.CreatedBy.Equals(userId));

            // Search
            if (!string.IsNullOrEmpty(query.SearchBy) && !string.IsNullOrEmpty(query.SearchValue))
            {
                if (query.SearchBy.Equals("name", StringComparison.OrdinalIgnoreCase))
                    brandQuery = brandQuery.Where(b => b.Name.Contains(query.SearchValue));
            }

            // Sort
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                bool desc = query.SortOrder?.ToLower() == "desc";
                brandQuery = query.SortBy.ToLower() switch
                {
                    "name" => desc ? brandQuery.OrderByDescending(b => b.Name) : brandQuery.OrderBy(b => b.Name),
                    "taxrate" => desc ? brandQuery.OrderByDescending(b => b.DefaultTaxRate) : brandQuery.OrderBy(b => b.DefaultTaxRate),
                    _ => brandQuery
                };
            }

            // Pagination
            var total = await brandQuery.CountAsync();
            var items = await brandQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<BrandAdminDTO>>(items);
            return new PagedResult<BrandAdminDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<bool> UpdateBrandAsync(Guid id, BrandAdminDTO dto, string userId)
        {
            CheckValidInput(userId);
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null || brand.IsDelete)
                return false;

            _mapper.Map(dto, brand);
            brand.UpdatedAt = DateTime.UtcNow;
            brand.UpdatedBy = userId;

            await _brandRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBrandAsync(Guid id, string userId)
        {
            CheckValidInput(userId);
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null || brand.IsDelete)
                return false;

            brand.IsDelete = true;
            brand.UpdatedAt = DateTime.UtcNow;
            brand.UpdatedBy = userId;

            await _brandRepository.SaveChangesAsync();
            return true;
        }

        public void CheckValidInput(string userId)
        {
            //check userId is not null or empty
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId is required(Please login).");
            }
        }
    }
}
