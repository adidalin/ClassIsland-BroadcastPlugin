using System.Text.Json.Serialization;

namespace ClassIsland.BroadcastPlugin.Models;

/// <summary>
/// 广播消息模型
/// </summary>
public class BroadcastMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("target_class")]
    public string TargetClass { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("alert_type")]
    public string AlertType { get; set; } = "fullscreen";
}
