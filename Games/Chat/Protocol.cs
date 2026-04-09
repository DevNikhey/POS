using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chat;

public class Envelope
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("pass")]
    public string? Pass { get; set; }

    [JsonPropertyName("room")]
    public string? Room { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);
    public static Envelope? FromJson(string json) => JsonSerializer.Deserialize<Envelope>(json);
}

public class ChatMessage
{
    public string User { get; set; } = "";
    public string Room { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Display => $"[{Timestamp:HH:mm:ss}] {User}: {Text}";
}
