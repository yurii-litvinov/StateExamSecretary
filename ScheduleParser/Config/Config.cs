using System.Text.Json.Serialization;

namespace ScheduleParser.Config;

public class Config 
{
    [JsonPropertyName("Расписание")]
    public required string Schedule { get; set; }
    [JsonPropertyName("Темы ВКР, бакалавры МОиАИС")]
    public required string BachelorsMs { get; set; }
    [JsonPropertyName("Темы ВКР, бакалавры ПИ")]
    public required string BachelorsSe { get; set; }
    [JsonPropertyName("Темы ВКР, магистры МОиАИС")]
    public required string MastersMs { get; set; }
    [JsonPropertyName("Темы ВКР, магистры ПИ")]
    public required string MastersSe { get; set; }
}