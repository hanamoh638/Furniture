using Microsoft.AspNetCore.Mvc;

namespace FurnitureAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API is live");
        }
    }
}
