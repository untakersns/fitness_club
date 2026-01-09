using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using fitness_club.DTO;
using fitness_club.Data;
using fitness_club.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace fitness_club.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FCDbContext _db;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SymmetricSecurityKey _signingKey;

        public AuthController(FCDbContext db, IConfiguration config, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, SymmetricSecurityKey signingKey)
        {
            _db = db;
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
            _signingKey = signingKey;
        }

        // POST api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (model == null)
                return BadRequest("Invalid payload.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                return BadRequest(new { errors = new { Email = new[] { "User with this email already exists." } } });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var res = await _userManager.CreateAsync(user, model.Password);
            if (!res.Succeeded)
            {
                var errors = res.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { errors = new { General = errors } });
            }

            return CreatedAtAction(null, null);
        }

        // POST api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null)
                return BadRequest("Invalid payload.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized();

            var signRes = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!signRes.Succeeded) return Unauthorized();

            // generate JWT using the centralized signing key from Program.cs
            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var jwtIssuer = _config["Jwt:Issuer"] ?? "fitness_club";
            var jwtAudience = _config["Jwt:Audience"] ?? "fitness_club_client";

            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = creds
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // create refresh token
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            var response = new TokenResponse
            {
                AccessToken = tokenString,
                RefreshToken = refreshToken.Token,
                ExpiresIn = 900 // 15 minutes
            };

            return Ok(response);
        }

        // POST api/Auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(nameId, out var userId)) return Unauthorized();

            // Аннулировать все refresh-токены пользователя
            var refreshTokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var rt in refreshTokens)
            {
                rt.IsRevoked = true;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST api/Auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.RefreshToken)) return BadRequest();

            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == model.RefreshToken && !r.IsRevoked);
            if (rt == null) return Unauthorized();
            if (rt.ExpiresAt < DateTime.UtcNow) return Unauthorized();

            var user = await _userManager.FindByIdAsync(rt.UserId.ToString());
            if (user == null) return Unauthorized();

            // generate new access token using the centralized signing key
            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var jwtIssuer = _config["Jwt:Issuer"] ?? "fitness_club";
            var jwtAudience = _config["Jwt:Audience"] ?? "fitness_club_client";

            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = creds
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new TokenResponse { AccessToken = tokenString, RefreshToken = rt.Token, ExpiresIn = 900 });
        }
    }
}
