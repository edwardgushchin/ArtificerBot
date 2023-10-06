namespace Artificer.Bot;

public class ChatResponse
{
    public ChatResponse(string reason, string content)
    {
        Content = content;
        FinishReason = reason;
    }
    
    public string Content { get; }
    public string FinishReason { get; }
}