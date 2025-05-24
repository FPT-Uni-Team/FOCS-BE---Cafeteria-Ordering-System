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

        protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}
