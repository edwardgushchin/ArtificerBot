namespace Artificer.Bot;

public class Message
{
    public string? role { get; set; }
    public string? content { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public int tokens { get; set; }
}