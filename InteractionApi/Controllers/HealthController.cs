using InteractionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IAgentStateServiceClient _agentStateService;
    private readonly IAuthServiceClient _authService;

    private readonly ISkillService _skillService;


    public HealthController(IAgentStateServiceClient agentStateService, IAuthServiceClient authService, ISkillService skillService)
    {
        _skillService = skillService;
        _agentStateService = agentStateService;
        _authService = authService;
    }


    [HttpGet("live")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "live",
            timestamp = DateTime.UtcNow,
            service = "interaction-api",
            description = "Skills management and agent activity coordination service"
        });
    }

    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {

        // Check if the interaction service is ready
        var isReady = await _skillService.IsServiceHealthyAsync() &&
                      await _agentStateService.IsServiceHealthyAsync() &&
                      await _authService.IsServiceHealthyAsync();

        if (!isReady)
        {
            return StatusCode(503, new { message = "Service is not ready" });
        }

        return Ok(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow,
            service = "interaction-api",
            description = "Skills management and agent activity coordination service is ready"
        });
    }
}