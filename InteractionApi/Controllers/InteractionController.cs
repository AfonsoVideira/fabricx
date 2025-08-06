using InteractionApi.Models;
using InteractionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Agent")]
public class InteractionController : ControllerBase
{
    private readonly IInteractionService _interactionService;
    private readonly IAuthServiceClient _authService;

    public InteractionController(IInteractionService interactionService, IAuthServiceClient authService)
    {
        _interactionService = interactionService;
        _authService = authService;
    }

    [HttpPut("start_do_not_disturb")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> StartDoNotDisturb([FromBody] NewInteractionRequest request)
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

        var agent = await _authService.GetUserByToken(authToken);

        if (agent == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Agent not found",
                Data = null
            });
        }

        var eventDto = new UpdateAgentActivityRequest
        {
            AgentId = int.Parse(agent.id), // Convert string id to int for request
            Action = "START_DO_NOT_DISTURB",
            Timestamp = request.Timestamp
        };

        var result = await _interactionService.UpdateAgentActivityAsync(authToken, eventDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("end_do_not_disturb")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> EndDoNotDisturb([FromBody] NewInteractionRequest request)
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

        var agent = await _authService.GetUserByToken(authToken);

        if (agent == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Agent not found",
                Data = null
            });
        }

        var eventDto = new UpdateAgentActivityRequest
        {
            AgentId = int.Parse(agent.id), // Convert string id to int for request
            Action = "END_DO_NOT_DISTURB",
            Timestamp = request.Timestamp
        };

        var result = await _interactionService.UpdateAgentActivityAsync(authToken, eventDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("start_call")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> StartCall([FromBody] NewInteractionRequest request)
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

        var agent = await _authService.GetUserByToken(authToken);

        if (agent == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Agent not found",
                Data = null
            });
        }

        var eventDto = new UpdateAgentActivityRequest
        {
            AgentId = int.Parse(agent.id), // Convert string id to int for request
            Action = "CALL_STARTED",
            Timestamp = request.Timestamp
        };

        var result = await _interactionService.UpdateAgentActivityAsync(authToken, eventDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("end_call")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> EndCall([FromBody] NewInteractionRequest request)
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

        var agent = await _authService.GetUserByToken(authToken);

        if (agent == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Agent not found",
                Data = null
            });
        }

        var eventDto = new UpdateAgentActivityRequest
        {
            AgentId = int.Parse(agent.id), // Convert string id to int for request
            Action = "CALL_ENDED",
            Timestamp = request.Timestamp
        };

        var result = await _interactionService.UpdateAgentActivityAsync(authToken, eventDto);

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