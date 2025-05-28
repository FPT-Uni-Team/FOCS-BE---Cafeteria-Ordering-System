using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FOCS.Controllers
{
    [ApiController]
    public class FocsController : ControllerBase
    {
        protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        protected string Email => User.FindFirstValue(ClaimTypes.Email);
        protected string Role => User.FindFirstValue(ClaimTypes.Role);

        protected string StoreId => User.FindFirstValue("StoreId");
        protected string AccessToken
        {
            get
            {
                var token = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").GetAwaiter().GetResult();
                return token ?? throw new InvalidOperationException("Access token not available.");
            }
        }

        protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}
