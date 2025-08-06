using System.ComponentModel.DataAnnotations;

namespace AgentStateApi.Models;

public class AgentStateEventDto
{
    public int AgentId { get; set; }
    
    [Required]
    public string EventType { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public List<string> Skills { get; set; } = new();
}

public class CreateAgentRequest
{
    [Required]
    public int UserId { get; set; } // User ID from AuthService
    
    [Required]
    public string AgentName { get; set; } = string.Empty;
    
    public string InitialState { get; set; } = "Available";
    
    public List<string> Skills { get; set; } = new();
}

public class CreateAgentByUserIdRequest
{
    [Required]
    public int UserId { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
} 