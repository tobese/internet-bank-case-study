using System.Text.Json.Serialization;

namespace InternetBankCalculator.Models;

[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(ApiErrorResponse))]
[JsonSerializable(typeof(MathematicianResponse))]
internal partial class AppJsonContext : JsonSerializerContext { }

public class ApiResponse
{
    [JsonPropertyName("input")]
    public int Input { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;
}

public class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}

public class MathematicianResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("era")]
    public string Era { get; set; } = string.Empty;
}
