using System.Net.Http.Json;
using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Client.Services;

/// <summary>
/// Centralized HTTP client service for API calls
/// Applies DRY principle and Facade pattern for API communication
/// Eliminates duplicate error handling across components
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Perform GET request with centralized error handling
    /// </summary>
    public async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        try
        {
            _logger.LogDebug("HTTP GET: {Url}", url);
            
            var response = await _http.GetFromJsonAsync<ApiResponse<T>>(url);
            
            if (response == null)
            {
                _logger.LogWarning("Empty response from {Url}", url);
                return ApiResponse<T>.ErrorResult("Empty response from server");
            }

            if (!response.Success)
            {
                _logger.LogWarning("API error from {Url}: {Message}", url, response.Message);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed: {Url}", url);
            return ApiResponse<T>.ErrorResult($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling {Url}", url);
            return ApiResponse<T>.ErrorResult($"Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Perform POST request with centralized error handling
    /// </summary>
    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        try
        {
            _logger.LogDebug("HTTP POST: {Url}", url);
            
            var httpResponse = await _http.PostAsJsonAsync(url, data);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("HTTP {StatusCode} from {Url}: {Error}", 
                    httpResponse.StatusCode, url, errorContent);
                return ApiResponse<TResponse>.ErrorResult($"HTTP {httpResponse.StatusCode}");
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
            
            if (response == null)
            {
                _logger.LogWarning("Empty response from {Url}", url);
                return ApiResponse<TResponse>.ErrorResult("Empty response from server");
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed: {Url}", url);
            return ApiResponse<TResponse>.ErrorResult($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling {Url}", url);
            return ApiResponse<TResponse>.ErrorResult($"Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Perform POST request expecting no response data
    /// </summary>
    public async Task<ApiResponse<object>> PostAsync<TRequest>(string url, TRequest data)
    {
        return await PostAsync<TRequest, object>(url, data);
    }
}
