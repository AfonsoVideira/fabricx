using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthServiceImpl> _logger;

    public AuthServiceImpl(AuthDbContext context, IJwtService jwtService, ILogger<AuthServiceImpl> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || user.Password != request.Password)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
                return null;
            }

            var token = _jwtService.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Assuming 24-hour expiration

            _logger.LogInformation("Successful login for user: {Username}", request.Username);

            return new TokenResponse
            {
                Token = token,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return null;
        }
    }

    public async Task<User?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if username or email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - username or email already exists: {Username}", request.Username);
                return null;
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password, // In production, this should be hashed
                IsAdmin = request.IsAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully registered user: {Username}", request.Username);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
            return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.Where(u => u.IsActive).OrderBy(u => u.Username).ToListAsync();
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deactivated user: {Username}", user.Username);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return false;
        }
    }

    public async Task<bool> IsServiceHealthyAsync()
    {
        try
        {
            // Check if the AuthService is reachable
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthService health check failed");
            return false;
        }
    }
} 