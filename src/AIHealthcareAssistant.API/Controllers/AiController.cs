using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        [HttpPost("chat")]
        public IActionResult Chat()
        {


            return Ok();        
        }

        [HttpPost("symptom-check")]
        public IActionResult SymptomCheck()
        {

            return Ok();
           
        }

        [HttpGet("health")]   
        public IActionResult Health()
        {
            return Ok();
          
        }
    }


}