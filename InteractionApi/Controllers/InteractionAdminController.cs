using InteractionApi.Models;
using InteractionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InteractionAdminController : ControllerBase
{
    private readonly IInteractionService _interactionService;

    public InteractionAdminController(IInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [HttpPut("agents/{agentId}/skills")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateAgentSkills(int agentId, [FromBody] List<string> skillIds)
    {
        var authToken = GetJwtTokenFromHeader();
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Authorization token required",
                Data = null
            });
        }

        var request = new UpdateAgentSkillsRequest
        {
            AgentId = agentId,
            SkillIds = skillIds
        };

        var result = await _interactionService.UpdateAgentSkillsAsync(authToken, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("agents/activity")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateAgentActivity([FromBody] UpdateAgentActivityRequest request)
    {
        var authToken = GetJwtTokenFromHeader();
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Authorization token required",
                Data = null
            });
        }

        var result = await _interactionService.UpdateAgentActivityAsync(authToken, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
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
} 