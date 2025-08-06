using InteractionApi.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InteractionApi.Services;

public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthServiceClient(HttpClient httpClient, ILogger<AuthServiceClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = configuration["Services:AuthService:BaseUrl"] ?? "http://auth-service:8080";
        _httpClient.BaseAddress = new Uri(baseUrl);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    }

    public async Task<UserInfo?> GetUserByToken(string authToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.GetAsync("/api/Auth/me");

            _logger.LogInformation("Requesting user by token with status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user by token with status: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserInfo>(responseJson, _jsonOptions);

            _logger.LogInformation("User {user}", user);

            if (user != null)
            {
                _logger.LogInformation("Successfully retrieved user by token: {Username} with ID: {UserId}", user.username, user.id);
            }
            else
            {
                _logger.LogWarning("No user found for the provided token.");
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by token");
            return null;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<bool> IsServiceHealthyAsync()
    {
        try
        {
            // Check if the AuthService is reachable
            return await _httpClient.GetAsync("/api/Health/live").ContinueWith(t => t.Result.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthService health check failed");
            return false;
        }
    }
} 