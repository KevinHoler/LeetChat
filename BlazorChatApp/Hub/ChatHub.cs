using Microsoft.AspNetCore.SignalR;
namespace BlazorChatApp.Hub
{
    public interface IChatHubClient
    {
        Task SendMessage(string user, string message);
        Task ReceiveMessage(string user, string message);
        Task StartConversation(string user1, string user2);
    }
    public class ChatHub : Hub<IChatHubClient>
    {
        public ChatHub()
        {
            
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
