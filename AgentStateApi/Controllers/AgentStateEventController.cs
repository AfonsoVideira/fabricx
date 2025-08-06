using AgentStateApi.Models;
using AgentStateApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AgentStateApi.Controllers;

[ApiController]
[Route("api/AgentState")]
[Authorize]
public class AgentStateEventController : ControllerBase
{
    private readonly IAgentStateService _agentStateService;
    private readonly ILogger<AgentStateEventController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AgentStateEventController(
        IAgentStateService agentStateService, 
        ILogger<AgentStateEventController> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _agentStateService = agentStateService;
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [HttpPost("event")]
    public async Task<IActionResult> ProcessEvent([FromBody] AgentStateEventDto eventDto)
    {
        try
        {
            await _agentStateService.ProcessAgentEventAsync(eventDto);
            return Ok(new { message = "Event processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing agent state event");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("agents")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentByUserIdRequest request)
    {
        try
        {
            // Get JWT token from current request
            var authToken = GetJwtTokenFromHeader();
            if (string.IsNullOrEmpty(authToken))
            {
                return Unauthorized(new { message = "Authorization token required" });
            }

            // Get user info from AuthService
            var user = await GetUserByUserIdAsync(request.UserId, authToken);
            if (user == null)
            {
                return NotFound(new { message = "User not found with the specified Id" });
            }

            // Create agent request
            var agentCreateRequest = new CreateAgentRequest
            {
                UserId = user.Id,
                AgentName = user.Username,
                InitialState = "Available",
                Skills = new List<string>() // Start with empty skills
            };

            var agent = await _agentStateService.CreateAgentAsync(agentCreateRequest);
            
            return Ok(new { 
                message = "Agent created successfully", 
                agent = new {
                    agent.Id,
                    agent.Name,
                    agent.UserId,
                    agent.State,
                    agent.LastStateChange,
                    Skills = new List<string>() // Always empty on creation
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent by UserId: {UserId}", request.UserId);
            return BadRequest(new { message = "Failed to create agent" });
        }
    }

    [HttpGet("agents")]
    public async Task<ActionResult<IEnumerable<object>>> GetAgents()
    {
        try
        {
            var agents = await _agentStateService.GetAllAgentsAsync();
            var agentResponses = agents.Select(agent => new {
                agent.Id,
                agent.Name,
                agent.UserId,
                agent.State,
                agent.LastStateChange,
                Skills = string.IsNullOrEmpty(agent.Skills) ? new List<string>() : 
                    System.Text.Json.JsonSerializer.Deserialize<List<string>>(agent.Skills)
            });
            
            return Ok(agentResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agents");
            return StatusCode(500, new { message = "Failed to retrieve agents" });
        }
    }

    [HttpGet("agents/{id}")]
    public async Task<ActionResult<object>> GetAgent(int id)
    {
        try
        {
            var agent = await _agentStateService.GetAgentByIdAsync(id);
            if (agent == null)
            {
                return NotFound(new { message = "Agent not found" });
            }

            return Ok(new {
                agent.Id,
                agent.Name,
                agent.UserId,
                agent.State,
                agent.LastStateChange,
                Skills = string.IsNullOrEmpty(agent.Skills) ? new List<string>() : 
                    System.Text.Json.JsonSerializer.Deserialize<List<string>>(agent.Skills)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent {AgentId}", id);
            return StatusCode(500, new { message = "Failed to retrieve agent" });
        }
    }

    [HttpGet("agents/user/{userId}")]
    public async Task<ActionResult<object>> GetAgentByUserId(int userId)
    {
        try
        {
            var agent = await _agentStateService.GetAgentByUserIdAsync(userId);
            if (agent == null)
            {
                return NotFound(new { message = "Agent not found for the specified user" });
            }

            return Ok(new {
                agent.Id,
                agent.Name,
                agent.UserId,
                agent.State,
                agent.LastStateChange,
                Skills = string.IsNullOrEmpty(agent.Skills) ? new List<string>() : 
                    System.Text.Json.JsonSerializer.Deserialize<List<string>>(agent.Skills)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent for UserId {UserId}", userId);
            return StatusCode(500, new { message = "Failed to retrieve agent" });
        }
    }

    private string? GetJwtTokenFromHeader()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }
        return null;
    }

    private async Task<UserInfo?> GetUserByUserIdAsync(int id, string authToken)
    {
        try
        {
            var authServiceBaseUrl = _configuration["Services:AuthService:BaseUrl"] ?? "http://auth-service:8080";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.GetAsync($"{authServiceBaseUrl}/api/auth/users");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get users from AuthService. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<IEnumerable<UserInfo>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var user = users?.FirstOrDefault(u => u.Id == id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Id from AuthService");
            return null;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
} 