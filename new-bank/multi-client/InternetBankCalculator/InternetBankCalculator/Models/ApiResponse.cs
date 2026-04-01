using System.Text.Json.Serialization;

namespace InternetBankCalculator.Models;

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
