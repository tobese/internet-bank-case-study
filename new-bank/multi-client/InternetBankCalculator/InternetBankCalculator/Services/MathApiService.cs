using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using InternetBankCalculator.Models;

namespace InternetBankCalculator.Services;

public class MathApiService
{
    private readonly HttpClient _httpClient;

    public MathApiService(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<string> GetFactorialAsync(int n)
    {
        var response = await _httpClient.GetAsync($"/api/fac/{n}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new Exception(error?.Error ?? "API error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result?.Result ?? "Error";
    }

    public async Task<string> GetFibonacciAsync(int n)
    {
        var response = await _httpClient.GetAsync($"/api/fib/{n}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new Exception(error?.Error ?? "API error");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result?.Result ?? "Error";
    }
}
