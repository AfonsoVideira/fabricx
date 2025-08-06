using InteractionApi.Models;

namespace InteractionApi.Services;

public interface IInteractionService
{
    Task<ApiResponse<object>> UpdateAgentSkillsAsync(string authToken, UpdateAgentSkillsRequest request);
    Task<ApiResponse<object>> UpdateAgentActivityAsync(string authToken, UpdateAgentActivityRequest request);

} 