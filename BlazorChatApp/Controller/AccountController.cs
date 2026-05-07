using Microsoft.AspNetCore.Mvc;
using BlazorChatApp.Data;
using BlazorChatApp.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using BlazorChatApp.Data.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace BlazorChatApp.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        public AccountController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellationToken)
        {
            var userNameExists = await _appDbContext.Users.AsNoTracking().AnyAsync(x => x.Username == dto.Username, cancellationToken);
            if (userNameExists)
            {
                return BadRequest("Username already exists.");
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash
            };

            _appDbContext.Users.Add(user);

            await _appDbContext.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == dto.Username, cancellationToken);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid username or password.");
            }
            
            return Ok("Login successful.");
        }
    }
}
