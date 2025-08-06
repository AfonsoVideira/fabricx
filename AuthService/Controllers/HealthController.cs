using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public HealthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpGet("live")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "live",
            timestamp = DateTime.UtcNow,
            service = "auth-service",
            description = "Authentication and authorization service"
        });
    }

    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {

        // Check if the interaction service is ready
        var isReady = await _authService.IsServiceHealthyAsync() && _jwtService.IsServiceHealthy();
                      

        if (!isReady)
        {
            return StatusCode(503, new { message = "Service is not ready" });
        }

        return Ok(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow,
            service = "auth-service",
            description = "Authentication and authorization service"
        });
    }
    
}
