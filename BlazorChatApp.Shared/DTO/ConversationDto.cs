public record ConversationDto(int Id, string Name, bool IsGroup, string LastMessage, DateTime LastActivity);
public record MessageDto(int Id, int ConversationId, string SenderUsername, string Content, DateTime Timestamp);
public record StartConversationDto(int TargetUserId);
public record UserSearchResultDto(int Id, string Username);
public record SendMessageDto(int ConversationId, string Content);