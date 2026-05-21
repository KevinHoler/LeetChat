using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BlazorChatApp.Data;
using BlazorChatApp.Data.Entities;
using BlazorChatApp.Shared.DTO;
using System.Security.Claims;

namespace BlazorChatApp.Hub
{
    public interface IChatHubClient
    {
        Task ReceiveMessage(MessageDto message);
        Task ConversationStarted(ConversationDto conversation);
    }

    [Authorize]
    public class ChatHub : Hub<IChatHubClient>
    {
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            var conversationIds = await _db.ConversationParticipants
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.ConversationId)
                .ToListAsync();

            foreach (var id in conversationIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{id}");

            await base.OnConnectedAsync();
        }

        public async Task JoinConversation(int conversationId)
        {
            var userId = GetUserId();
            var isParticipant = await _db.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (isParticipant)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
        }

        public async Task SendMessage(int conversationId, string content)
        {
            var userId = GetUserId();

            var isParticipant = await _db.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant) return;

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = content.Trim(),
                Timestamp = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            var username = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Username)
                .FirstAsync();

            var dto = new MessageDto(message.Id, conversationId, username, message.Content, message.Timestamp);
            await Clients.Group($"conv_{conversationId}").ReceiveMessage(dto);
        }

        private int GetUserId() =>
            int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}