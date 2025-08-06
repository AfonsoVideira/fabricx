using AgentStateApi.Models;

namespace AgentStateApi.Services;

public interface IAgentStateService
{
    Task ProcessAgentEventAsync(AgentStateEventDto eventDto);
    Task<Agent> CreateAgentAsync(CreateAgentRequest request);
    Task<Agent?> GetAgentByIdAsync(int agentId);
    Task<Agent?> GetAgentByUserIdAsync(int userId);
    Task<IEnumerable<Agent>> GetAllAgentsAsync();
    Task<bool> IsServiceHealthyAsync();
} 