using AgentStateApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentStateApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthController : ControllerBase
{
    private readonly IAgentStateService _agentStateService;

    public HealthController(IAgentStateService agentStateService)
    {
        _agentStateService = agentStateService;
    }

    [HttpGet("live")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "live",
            timestamp = DateTime.UtcNow,
            service = "agent-state-api",
            description = "Agent state management service",
            hostname= System.Net.Dns.GetHostName(),
        });
    }

    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {

        // Check if the interaction service is ready
        var isReady = await _agentStateService.IsServiceHealthyAsync();

        if (!isReady)
        {
            return StatusCode(503, new { message = "Service is not ready" });
        }

        return Ok(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow,
            service = "agent-state-api",
            description = "Agent state management service is ready"
        });
    }
    

} 