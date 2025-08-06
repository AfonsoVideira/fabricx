using System.ComponentModel.DataAnnotations;

namespace AgentStateApi.Models;

public class Agent
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int UserId { get; set; } // Soft foreign key to AuthService.Users.Id
    
    public string State { get; set; } = "Available";
    
    public DateTime LastStateChange { get; set; } = DateTime.UtcNow;
    
    // JSON string to store skills array
    public string? Skills { get; set; }
} 