using Microsoft.AspNetCore.Mvc;
using BlazorChatApp.Data;
using BlazorChatApp.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using BlazorChatApp.Data.Entities;

namespace BlazorChatApp.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly TokenService _tokenService;
        public AccountController(AppDbContext appDbContext, TokenService tokenService)
        {
            _appDbContext = appDbContext;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
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
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Users.AddAsync(user, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return Ok(GenerateToken(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Username == dto.Username, cancellationToken);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(GenerateToken(user));
        }

        private AuthResponseDto GenerateToken(User user)
        {
            var token = _tokenService.GenerateJWT(user);
            return new AuthResponseDto(user.Username, token);
        }
    }
}
