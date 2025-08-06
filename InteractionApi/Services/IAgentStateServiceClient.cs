using InteractionApi.Models;

namespace InteractionApi.Services;

public interface IAgentStateServiceClient
{
    Task<bool> UpdateAgentSkillsAsync(int agentId, List<string> skillIds, string authToken);
    Task<bool> UpdateAgentActivityAsync(AgentStateEventDto eventDto, string authToken);

    Task<bool> IsServiceHealthyAsync();
} 