using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : FocsController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        public async Task<MenuCategoryDTO> Create(CreateCategoryRequest request)
        {   
            return await _categoryService.CreateCategoryAsync(request, StoreId);
        }

        [HttpPost("disable/{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        public async Task<bool> Disable(Guid id)
        { 
            return await _categoryService.DisableCategory(id, StoreId);
        }

        [HttpPost("enable/{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        public async Task<bool> Enable(Guid id)
        {
            return await _categoryService.EnableCategory(id, StoreId);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        public async Task<MenuCategoryDTO> Update(Guid id, UpdateCategoryRequest updateCategoryRequest)
        {
            return await _categoryService.UpdateCategoryAsync(updateCategoryRequest, id, StoreId);
        }

        [HttpPost("categories")]
        public async Task<PagedResult<MenuCategoryDTO>> ListCategories([FromBody] UrlQueryParameters urlQueryParameters)
        {
            return await _categoryService.ListCategoriesAsync(urlQueryParameters, StoreId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cate = await _categoryService.GetById(id, Guid.Parse(StoreId));
            
            if(cate == null) return NotFound();

            return Ok(cate);
        }

        
    }
}
