using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalSolutions.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")] //comment this to go into  debug mode thru swagger
    [Produces("application/json")]
    [Route("api/Ping")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class PingController : Controller
    {
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpGet("PingToken")]
        [ProducesResponseType(200)]
        public IActionResult PingToken()
        {
            return Ok();
        }
    }
}
