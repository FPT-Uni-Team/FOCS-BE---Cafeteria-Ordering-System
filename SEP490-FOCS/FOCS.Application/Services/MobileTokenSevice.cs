using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class MobileTokenSevice : IMobileTokenSevice
    {
        private readonly IRepository<MobileTokenDevice> _mobileTokenDevice;

        public MobileTokenSevice(IRepository<MobileTokenDevice> mobileTokenDevice)
        {
            _mobileTokenDevice = mobileTokenDevice;
        }

        public async Task<MobileTokenRequest> GetMobileToken(Guid userId)
        {
            var tokenDevice = await _mobileTokenDevice.AsQueryable().FirstOrDefaultAsync(x => x.UserId == userId);

            return new MobileTokenRequest
            {
                ActorId = tokenDevice.UserId,
                Token = tokenDevice.Token,
                DeviceId = tokenDevice.DeviceId,
                Platform = tokenDevice.Platform
            };
        }
    }
}
