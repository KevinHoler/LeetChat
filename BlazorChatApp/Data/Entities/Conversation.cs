namespace BlazorChatApp.Data.Entities
{
    public class Conversation
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Message> Messages { get; set; } = new();
        public List<ConversationParticipant> Participants { get; set; } = new();
        public string Name { get; set; } = string.Empty;
        public bool IsGroup { get; set; } = false;
    }
    public class ConversationParticipant
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
