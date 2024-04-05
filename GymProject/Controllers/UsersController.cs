using GymProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddUser(Customer customer)
        {
            return Ok();
        }
    }
}
