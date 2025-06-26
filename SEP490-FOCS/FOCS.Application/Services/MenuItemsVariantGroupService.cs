using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Esf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static FOCS.Common.Exceptions.Errors;

namespace FOCS.Application.Services
{
    public class MenuItemsVariantGroupService : IMenuItemsVariantGroupService
    {
        private readonly IRepository<MenuItemVariantGroup> _menuItemVariantGroupRepository;

        private readonly IMapper _mapper;

        public MenuItemsVariantGroupService(IMapper mapper, IRepository<MenuItemVariantGroup> menuItemVariantGroupRepo)
        {
            _menuItemVariantGroupRepository = menuItemVariantGroupRepo;
            _mapper = mapper;
        }

        public async Task<List<MenuItemVariantGroup>> AssignMenuItemToVariantGroup(CreateMenuItemVariantGroupRequest request)
        {
            try
            {
                var newMenuItemVariantGroups = request.VariantGroupIds
                                           .Select(variantGroupId => new MenuItemVariantGroup
                                           {
                                               Id = Guid.NewGuid(),
                                               MenuItemId = request.MenuItemId.Value,
                                               VariantGroupId = variantGroupId,
                                               MinSelect = request.MinSelect,
                                               MaxSelect = request.MaxSelect,
                                               IsRequired = request.IsRequired
                                           })
                                           .ToList();

                await _menuItemVariantGroupRepository.AddRangeAsync(newMenuItemVariantGroups);
                await _menuItemVariantGroupRepository.SaveChangesAsync();

                return newMenuItemVariantGroups;
            } catch (Exception ex)
            {
                return new List<MenuItemVariantGroup>();
            }
        }

        public async Task<GetVariantGroupAndVariantResponse> GetVariantGroupsWithVariants(Guid menuItemId, Guid storeId)
        {
            var variantGroups = await _menuItemVariantGroupRepository.AsQueryable().Include(x => x.VariantGroup).
                                                                                    ThenInclude(x => x.Variants.Where(x => x.CreatedBy == storeId.ToString())).
                                                                                    Where(x => x.MenuItemId == menuItemId).ToListAsync();

            ConditionCheck.CheckCondition(variantGroups != null, Errors.Common.NotFound);

            var rs = new GetVariantGroupAndVariantResponse
            {
                Group = new Dictionary<string, List<GetVariantGroupResponse>>()
            };

            foreach (var groupWrapper in variantGroups)
            {
                var groupName = groupWrapper.VariantGroup.Name;

                if (!rs.Group.ContainsKey(groupName))
                {
                    rs.Group[groupName] = new List<GetVariantGroupResponse>();
                }

                foreach (var variant in groupWrapper.VariantGroup.Variants)
                {
                    if (!rs.Group[groupName].Any(v => v.Name == variant.Name))
                    {
                        rs.Group[groupName].Add(new GetVariantGroupResponse { Name = variant.Name });
                    }
                }
            }

            return rs;
        }
    }
}
