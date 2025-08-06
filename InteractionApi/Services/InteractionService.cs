using InteractionApi.Models;
using InteractionApi.Services;

namespace InteractionApi.Services;

public class InteractionService : IInteractionService
{
    private readonly IAgentStateServiceClient _agentStateServiceClient;
    private readonly ISkillService _skillService;
    private readonly ILogger<InteractionService> _logger;

    public InteractionService(
        IAgentStateServiceClient agentStateServiceClient,
        ISkillService skillService,
        ILogger<InteractionService> logger)
    {
        _agentStateServiceClient = agentStateServiceClient;
        _skillService = skillService;
        _logger = logger;
    }

    public async Task<ApiResponse<object>> UpdateAgentSkillsAsync(string authToken, UpdateAgentSkillsRequest request)
    {
        try
        {
            // Loop through each skill ID and check if it exists
            foreach (var skillId in request.SkillIds)
            {
                // convert skillId into int
                var skill = await _skillService.GetSkillByIdAsync(int.Parse(skillId));
                if (skill == null)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Skill with ID {skillId} does not exist",
                        Data = null
                    };
                }
            }

            // Call AgentStateApi to update agent skills
            var success = await _agentStateServiceClient.UpdateAgentSkillsAsync(request.AgentId, request.SkillIds, authToken);

            if (!success)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update agent skills",
                    Data = null
                };
            }

            _logger.LogInformation("Successfully updated skills for agent {AgentId}", request.AgentId);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Agent skills updated successfully",
                Data = new
                {
                    AgentId = request.AgentId,
                    UpdatedSkills = request.SkillIds
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skills for agent {AgentId}", request.AgentId);
            return new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating agent skills",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<object>> UpdateAgentActivityAsync(string authToken, UpdateAgentActivityRequest request)
    {
        try
        {

            // Loop through each skill ID and check if it exists
            foreach (var skillId in request.SkillIds)
            {
                // convert skillId into int
                var skill = await _skillService.GetSkillByIdAsync(int.Parse(skillId));
                if (skill == null)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Skill with ID {skillId} does not exist",
                        Data = null
                    };
                }
            }

            // Create agent state event DTO
            var eventDto = new AgentStateEventDto
            {
                AgentId = request.AgentId,
                EventType = request.Action,
                Timestamp = request.Timestamp,
                Skills = request.SkillIds
            };

            // Call AgentStateApi to update agent activity
            var success = await _agentStateServiceClient.UpdateAgentActivityAsync(eventDto, authToken);

            if (!success)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update agent activity",
                    Data = null
                };
            }

            _logger.LogInformation("Successfully updated activity for agent {AgentId} with action {Action}",
                request.AgentId, request.Action);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Agent activity updated successfully",
                Data = new
                {
                    AgentId = request.AgentId,
                    Action = request.Action,
                    Timestamp = request.Timestamp,
                    Skills = request.SkillIds,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity for agent {AgentId}", request.AgentId);
            return new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating agent activity",
                Data = null
            };
        }
    }
} 