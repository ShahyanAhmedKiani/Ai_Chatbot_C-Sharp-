namespace AIChatbot.Models
{
    /// <summary>
    /// Represents a single chat message in the conversation.
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;       // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ChatMessage() { }

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
            Timestamp = DateTime.Now;
        }

        public bool IsUser => Role == "user";
        public bool IsAssistant => Role == "assistant";

        public override string ToString() =>
            $"[{Timestamp:HH:mm:ss}] {Role.ToUpper()}: {Content}";
    }
}
