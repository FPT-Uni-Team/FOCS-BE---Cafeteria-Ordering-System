using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FOCS.Controllers
{
    [ApiController]
    public class FocsController : ControllerBase
    {
        protected string UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        protected string Email => User?.FindFirst(ClaimTypes.Email)?.Value;
        protected string Role => User?.FindFirst(ClaimTypes.Role)?.Value;

        protected string StoreId => HttpContext.Request.Headers["StoreId"].ToString();

        protected string TableId => HttpContext.Request.Headers["TableId"].ToString();

        protected string AccessToken =>
            HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        [NonAction]
        public IActionResult DebugClaims()
        {
            var claims = User?.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
    }
}
