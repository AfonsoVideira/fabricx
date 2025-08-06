using AgentStateApi.Data;
using AgentStateApi.Exceptions;
using AgentStateApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AgentStateApi.Services;

public class AgentStateService : IAgentStateService
{
    private readonly AgentStateDbContext _context;
    private readonly ILogger<AgentStateService> _logger;

    public AgentStateService(AgentStateDbContext context, ILogger<AgentStateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessAgentEventAsync(AgentStateEventDto eventDto)
    {
        try
        {
            // Validate timestamp - reject events older than 60 minutes
            var oneHourAgo = DateTime.UtcNow.AddMinutes(-60);
            if (eventDto.Timestamp < oneHourAgo)
            {
                throw new LateEventException($"Event timestamp {eventDto.Timestamp} is too old. Events must be within the hour.");
            }

            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == eventDto.AgentId);
            
            if (agent == null)
            {
                _logger.LogWarning("Agent with ID {AgentId} not found for event {EventType}",
                    eventDto.AgentId, eventDto.EventType);
                throw new InvalidOperationException($"Agent with ID {eventDto.AgentId} not found.");
            }

            // Update skills if provided
            if (eventDto.Skills != null && eventDto.Skills.Any())
            {
                agent.Skills = JsonSerializer.Serialize(eventDto.Skills);
            }

            var newState = MapEventTypeToState(eventDto.EventType, eventDto.Timestamp);
            if (newState == null)
            {
                throw new InvalidOperationException($"Unknown event type: {eventDto.EventType}");
            }

            agent.State = newState;
            agent.LastStateChange = eventDto.Timestamp;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Processed event {EventType} for agent {AgentId}", 
                eventDto.EventType, eventDto.AgentId);
        }
        catch (LateEventException)
        {
            throw; // Re-throw late event exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventType} for agent {AgentId}", 
                eventDto.EventType, eventDto.AgentId);
            throw;
        }
    }

    public async Task<Agent> CreateAgentAsync(CreateAgentRequest request)
    {
        try
        {
            // Check if agent with this UserId already exists
            var existingAgent = await _context.Agents
                .FirstOrDefaultAsync(a => a.UserId == request.UserId);
            
            if (existingAgent != null)
            {
                throw new InvalidOperationException($"Agent already exists for UserId {request.UserId}");
            }

            var agent = new Agent
            {
                Name = request.AgentName,
                UserId = request.UserId, // Set the soft foreign key
                State = request.InitialState,
                LastStateChange = DateTime.UtcNow
            };

            // Add skills if provided
            if (request.Skills.Any())
            {
                agent.Skills = JsonSerializer.Serialize(request.Skills);
            }

            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created agent {AgentName} with ID {AgentId} for UserId {UserId}", 
                agent.Name, agent.Id, agent.UserId);

            return agent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent for UserId {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<Agent?> GetAgentByIdAsync(int agentId)
    {
        return await _context.Agents.FirstOrDefaultAsync(a => a.Id == agentId);
    }

    public async Task<Agent?> GetAgentByUserIdAsync(int userId)
    {
        return await _context.Agents.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<IEnumerable<Agent>> GetAllAgentsAsync()
    {
        return await _context.Agents.OrderBy(a => a.Name).ToListAsync();
    }

    private static string? MapEventTypeToState(string eventType, DateTime eventTimestamp)
    {

        if (eventTimestamp.Hour >= 11 && eventTimestamp.Hour < 13 && eventType == "START_DO_NOT_DISTURB")
        {
            return "ON_LUNCH";
        }

        return eventType switch
        {
            "START_DO_NOT_DISTURB" => "DO_NOT_DISTURB",
            "END_DO_NOT_DISTURB" => "AVAILABLE",
            "CALL_STARTED" => "ON_CALL",
            "CALL_ENDED" => "AVAILABLE",
            _ => null // Unknown event types don't change state
        };
    }
} 