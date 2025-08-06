using System.ComponentModel.DataAnnotations;

namespace InteractionApi.Models;

public class Skill
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateSkillRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateSkillRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateAgentSkillsRequest
{
    [Required]
    public int AgentId { get; set; }
    
    [Required]
    public List<string> SkillIds { get; set; } = new();
}

public class UpdateAgentActivityRequest
{
    [Required]
    public int AgentId { get; set; }

    [Required]
    public string Action { get; set; } = string.Empty; // "START_DO_NOT_DISTURB", "CALL_STARTED", etc.

    [Required]
    public DateTime Timestamp { get; set; }

    public List<string> SkillIds { get; set; } = new();
}

public class NewInteractionRequest
{
    [Required]
    public DateTime Timestamp { get; set; }
}

public class AgentStateEventDto
{
    public int AgentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> Skills { get; set; } = new();
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
} 