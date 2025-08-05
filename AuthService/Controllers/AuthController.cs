using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _authService.RegisterAsync(request);
        if (user == null)
        {
            return BadRequest(new { message = "Username or email already exists" });
        }

        // Don't return the password in the response
        var userResponse = new
        {
            user.Id,
            user.Username,
            user.Email,
            user.IsAdmin,
            user.IsActive,
            user.CreatedAt
        };

        return Ok(userResponse);
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
    {
        var isValid = _jwtService.ValidateToken(request.Token);
        return Ok(new { isValid });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        
        // Don't return passwords in the response
        var userResponses = users.Select(user => new
        {
            user.Id,
            user.Username,
            user.Email,
            user.IsAdmin,
            user.IsActive,
            user.CreatedAt
        });

        return Ok(userResponses);
    }

    [HttpGet("users/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Don't return the password in the response
        var userResponse = new
        {
            user.Id,
            user.Username,
            user.Email,
            user.IsAdmin,
            user.IsActive,
            user.CreatedAt
        };

        return Ok(userResponse);
    }

    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var success = await _authService.DeleteUserAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            id = userId,
            username = username,
            email = email,
            role = role,
            isAdmin = role == "Admin",
            // For compatibility with existing code that expects firstName and lastName
            firstName = username?.Split(' ').FirstOrDefault() ?? "",
            lastName = username?.Split(' ').Skip(1).FirstOrDefault() ?? ""
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, service = "auth-service" });
    }
}

public class TokenValidationRequest
{
    public string Token { get; set; } = string.Empty;
} 