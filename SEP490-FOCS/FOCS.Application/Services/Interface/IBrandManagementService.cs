using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IBrandManagementService
    {
        Task<BrandAdminServiceDTO> CreateBrandAsync(CreateAdminBrandRequest dto, string userId);
        Task<PagedResult<BrandAdminServiceDTO>> GetAllBrandsAsync(UrlQueryParameters query);
        Task<bool> UpdateBrandAsync(Guid id, BrandAdminServiceDTO dto, string userId);
        Task<bool> DeleteBrandAsync(Guid id, string userId);
    }
}
