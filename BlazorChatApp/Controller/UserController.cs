// BlazorChatApp/Controllers/UserController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorChatApp.Data;
using BlazorChatApp.Shared.DTO;
using System.Security.Claims;

namespace BlazorChatApp.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db) => _db = db;

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(Array.Empty<UserSearchResultDto>());

            var results = await _db.Users
                .Where(u => u.Id != userId && EF.Functions.ILike(u.Username, $"%{q}%"))
                .Take(10)
                .Select(u => new UserSearchResultDto(u.Id, u.Username))
                .ToListAsync(ct);

            return Ok(results);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}