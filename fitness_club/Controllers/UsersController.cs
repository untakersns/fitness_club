using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using fitness_club.DTO;
using Microsoft.AspNetCore.Authorization;
using fitness_club.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using fitness_club.Entities;

namespace fitness_club.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly FCDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(FCDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET api/Users/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(nameId, out var id)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var dto = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Balance = user.Balance
            };

            return Ok(dto);
        }
    }
}
