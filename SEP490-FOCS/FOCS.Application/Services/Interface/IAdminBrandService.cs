using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;

namespace FOCS.Application.Services.Interface
{
    public interface IAdminBrandService
    {
        Task<BrandAdminServiceDTO> CreateBrandAsync(CreateAdminBrandRequest dto, string userId);
        Task<PagedResult<BrandAdminServiceDTO>> GetAllBrandsAsync(UrlQueryParameters query, string userId);
        Task<bool> UpdateBrandAsync(Guid id, BrandAdminServiceDTO dto, string userId);
        Task<bool> DeleteBrandAsync(Guid id, string userId);
    }
}
