using System.ComponentModel.DataAnnotations;

namespace BlazorChatApp.Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(25)]
        public string Username { get; set; } = string.Empty;
  
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ConversationParticipant> Conversations { get; set; } = new();
    }
    
}
