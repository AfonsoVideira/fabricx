using InteractionApi.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InteractionApi.Services;

public class AgentStateServiceClient : IAgentStateServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentStateServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AgentStateServiceClient(HttpClient httpClient, ILogger<AgentStateServiceClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = configuration["Services:AgentStateApi:BaseUrl"] ?? "http://agent-state-api:8080";
        _httpClient.BaseAddress = new Uri(baseUrl);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<bool> UpdateAgentSkillsAsync(int agentId, List<string> skillIds, string authToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Create agent state event for skills update
            var eventDto = new AgentStateEventDto
            {
                AgentId = agentId,
                EventType = "SKILLS_UPDATE",
                Timestamp = DateTime.UtcNow,
                Skills = skillIds
            };

            var json = JsonSerializer.Serialize(eventDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/AgentState/event", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update agent skills. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Successfully updated skills for agent {AgentId}", agentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent skills for agent {AgentId}", agentId);
            return false;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<bool> UpdateAgentActivityAsync(AgentStateEventDto eventDto, string authToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var json = JsonSerializer.Serialize(eventDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/AgentState/event", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update agent activity. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Successfully updated activity for agent {AgentId} with action {Action}",
                eventDto.AgentId, eventDto.EventType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent activity for agent {AgentId}", eventDto.AgentId);
            return false;
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
            // Check if the AgentStateApi is reachable
            return await _httpClient.GetAsync("/api/Health/live").ContinueWith(t => t.Result.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AgentStateApi health check failed");
            return false;
        }
    }
} 