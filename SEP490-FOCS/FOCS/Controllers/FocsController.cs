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
        // Property to retrieve the UserId from the claims
        protected string UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Property to retrieve the User's Email from the claims
        protected string Email => User?.FindFirstValue(ClaimTypes.Email);

        // Property to retrieve the User's Role from the claims
        protected string Role => User?.FindFirstValue(ClaimTypes.Role);

        // Property to retrieve StoreId from the request headers
        protected string StoreId
        {
            get
            {
                var storeId = HttpContext.Request.Headers["StoreId"].ToString();
                if (string.IsNullOrWhiteSpace(storeId))
                {
                    // Optional: Log the issue here or throw a custom exception
                    throw new InvalidOperationException("StoreId header is missing.");
                }
                return storeId;
            }
        }

        // Async method to get the Access Token from the Authorization header
        protected async Task<string> GetAccessTokenAsync()
        {
            var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Access token not available.");
            }
            return token;
        }

        // Method to parse the JWT token and extract the claims
        protected JwtSecurityToken GetJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken;
        }

        // Method to extract claims from the JWT
        protected string GetClaimFromToken(JwtSecurityToken jwtToken, string claimType)
        {
            var claim = jwtToken?.Claims.FirstOrDefault(c => c.Type == claimType);
            return claim?.Value;
        }

        // Property to check if the user is authenticated
        protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        // Example action that demonstrates usage
        protected async Task<IActionResult> GetClaimsAsync()
        {
            var token = await GetAccessTokenAsync(); // Get the access token asynchronously
            var jwtToken = GetJwtToken(token); // Parse the token
            var userId = GetClaimFromToken(jwtToken, ClaimTypes.NameIdentifier);
            var email = GetClaimFromToken(jwtToken, ClaimTypes.Email);
            var role = GetClaimFromToken(jwtToken, ClaimTypes.Role);
            var storeId = GetClaimFromToken(jwtToken, "StoreId");

            // Return the claims as an example response
            return Ok(new { UserId = userId, Email = email, Role = role, StoreId = storeId });
        }
    }
}
