using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIHealthcareAssistant.Infrastructure.Persistence;

namespace AIHealthcareAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {

  
        [HttpGet]
        public IActionResult GetAll()
        {
            

            return Ok();
        }

    
        [HttpGet("{id:int}")]
        public IActionResult GetById()
        {
          

            return Ok();
        }

     
        [HttpPost]
        public IActionResult Create()
        {
          

            return Ok();
        }

        [HttpPut("{id:int}")]
        public IActionResult Update()
        {
           
            return Ok();
        }

       
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            

            return Ok();
        }
    }
}