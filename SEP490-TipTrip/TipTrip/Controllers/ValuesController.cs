using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TipTrip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ValuesController : ControllerBase
    {
        protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        protected string Email => User.FindFirstValue(ClaimTypes.Email);
        protected string Role => User.FindFirstValue(ClaimTypes.Role); 

        protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}
