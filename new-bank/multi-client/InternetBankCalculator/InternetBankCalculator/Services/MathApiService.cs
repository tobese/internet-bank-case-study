using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using InternetBankCalculator.Models;

namespace InternetBankCalculator.Services;

public class MathApiService
{
    private readonly HttpClient _httpClient;

    public MathApiService(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<string> GetFactorialAsync(int n, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/fac/{n}", ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.ApiErrorResponse, ct);
            throw new Exception(error?.Error ?? "API error");
        }
        var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.ApiResponse, ct);
        return result?.Result ?? "Error";
    }

    public async Task<string> GetFibonacciAsync(int n, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/fib/{n}", ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.ApiErrorResponse, ct);
            throw new Exception(error?.Error ?? "API error");
        }
        var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.ApiResponse, ct);
        return result?.Result ?? "Error";
    }

    public async IAsyncEnumerable<int> StreamPrimesAsync(int limit, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/primes/sieve/{limit}");
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        string? line;
        while (!ct.IsCancellationRequested && (line = await reader.ReadLineAsync(ct)) != null)
        {
            if (line.StartsWith("data:"))
            {
                var valueStr = line["data:".Length..].Trim();
                if (int.TryParse(valueStr, out var prime))
                    yield return prime;
            }
        }
    }

    public async Task<MathematicianResponse?> GetDailyMathematicianAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/mathematician/daily", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync(AppJsonContext.Default.MathematicianResponse, ct);
    }
}
