using FOCS.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api")]
    [ApiController]
    public class OrdersController : FocsController
    {
        public OrdersController() { }

        [HttpPost("test")]
        [Authorize(Roles = Roles.User)]
        public IActionResult Test(int a, int b)
        {
            return a > b ? Ok() : BadRequest();
        }

    }
}
