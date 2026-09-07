using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return Ok();
           
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            return Ok();
          
        }

        [HttpGet("system-status")]
        public IActionResult GetSystemStatus()
        {
            return Ok();
           
        }
    }
}