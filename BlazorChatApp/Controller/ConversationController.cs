// BlazorChatApp/Controllers/ConversationController.cs
using BlazorChatApp.Data;
using BlazorChatApp.Data.Entities;
using BlazorChatApp.Hub;
using BlazorChatApp.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazorChatApp.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<ChatHub, IChatHubClient> _hub;

        public ConversationController(AppDbContext db, IHubContext<ChatHub, IChatHubClient> hub)
        {
            _db = db;
            _hub = hub;
        }

        // Hämta alla konversationer för inloggad användare
        [HttpGet]
        public async Task<IActionResult> GetConversations(CancellationToken ct)
        {
            var userId = GetUserId();

            var conversations = await _db.ConversationParticipants
                .Where(cp => cp.UserId == userId)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Participants)
                        .ThenInclude(p => p.User)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Messages.OrderByDescending(m => m.Timestamp).Take(1))
                .Select(cp => cp.Conversation)
                .ToListAsync(ct);

            var dtos = conversations
                .OrderByDescending(c => c.Messages.MaxBy(m => m.Timestamp)?.Timestamp ?? c.CreatedAt)
                .Select(c =>
                {
                    var name = c.IsGroup
                        ? c.Name
                        : c.Participants.First(p => p.UserId != userId).User.Username;
                    var last = c.Messages.MaxBy(m => m.Timestamp);
                    return new ConversationDto(c.Id, name, c.IsGroup, last?.Content ?? "", last?.Timestamp ?? c.CreatedAt);
                });

            return Ok(dtos);
        }

        // Hämta meddelanden för en konversation (med paginering)
        [HttpGet("{id:int}/messages")]
        public async Task<IActionResult> GetMessages(int id, [FromQuery] int limit = 50, [FromQuery] int? beforeId = null, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var isParticipant = await _db.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == id && cp.UserId == userId, ct);

            if (!isParticipant) return Forbid();

            var query = _db.Messages.Where(m => m.ConversationId == id);
            if (beforeId.HasValue)
                query = query.Where(m => m.Id < beforeId.Value);

            var messages = await query
                .OrderByDescending(m => m.Id)
                .Take(limit)
                .Include(m => m.Sender)
                .OrderBy(m => m.Id)
                .Select(m => new MessageDto(m.Id, m.ConversationId, m.Sender.Username, m.Content, m.Timestamp))
                .ToListAsync(ct);

            return Ok(messages);
        }

        // Starta (eller hämta befintlig) 1-till-1 konversation
        [HttpPost("start")]
        public async Task<IActionResult> StartConversation(StartConversationDto dto, CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == dto.TargetUserId)
                return BadRequest("Du kan inte chatta med dig själv.");

            var targetUser = await _db.Users.FindAsync([dto.TargetUserId], ct);
            if (targetUser == null) return NotFound("Användaren hittades inte.");

            // Kolla om 1-till-1 konversation redan finns
            var existingId = await _db.ConversationParticipants
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.ConversationId)
                .Intersect(
                    _db.ConversationParticipants
                        .Where(cp => cp.UserId == dto.TargetUserId)
                        .Select(cp => cp.ConversationId)
                )
                .Join(_db.Conversations.Where(c => !c.IsGroup), id => id, c => c.Id, (id, c) => c.Id)
                .FirstOrDefaultAsync(ct);

            if (existingId != 0)
            {
                var existing = await _db.Conversations.FindAsync([existingId], ct);
                return Ok(new ConversationDto(existingId, targetUser.Username, false, "", existing!.CreatedAt));
            }

            // Skapa ny konversation
            var conversation = new Conversation { CreatedAt = DateTime.UtcNow };
            _db.Conversations.Add(conversation);
            await _db.SaveChangesAsync(ct);

            _db.ConversationParticipants.AddRange(
                new ConversationParticipant { ConversationId = conversation.Id, UserId = userId },
                new ConversationParticipant { ConversationId = conversation.Id, UserId = dto.TargetUserId }
            );
            await _db.SaveChangesAsync(ct);

            var myUsername = await _db.Users.Where(u => u.Id == userId).Select(u => u.Username).FirstAsync(ct);
            var convDto = new ConversationDto(conversation.Id, targetUser.Username, false, "", conversation.CreatedAt);
            var targetConvDto = new ConversationDto(conversation.Id, myUsername, false, "", conversation.CreatedAt);

            // Notifiera mottagaren om den är online
            await _hub.Clients.Group($"user_{dto.TargetUserId}").ConversationStarted(targetConvDto);

            return Ok(convDto);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}