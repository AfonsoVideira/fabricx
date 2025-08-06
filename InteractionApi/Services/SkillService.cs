using InteractionApi.Data;
using InteractionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InteractionApi.Services;

public class SkillService : ISkillService
{
    private readonly InteractionDbContext _context;
    private readonly ILogger<SkillService> _logger;

    public SkillService(InteractionDbContext context, ILogger<SkillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Skill>> GetAllSkillsAsync()
    {
        return await _context.Skills
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Skill>> GetActiveSkillsAsync()
    {
        return await _context.Skills
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Skill?> GetSkillByIdAsync(int id)
    {
        return await _context.Skills.FindAsync(id);
    }

    public async Task<Skill> CreateSkillAsync(CreateSkillRequest request)
    {
        _logger.LogInformation("Creating new skill: {SkillName}", request.Name);

        // Check if skill with same name already exists
        var existingSkill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == request.Name.ToLower());

        if (existingSkill != null)
        {
            throw new ArgumentException($"Skill with name '{request.Name}' already exists");
        }

        var skill = new Skill
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created skill {SkillId}: {SkillName}", skill.Id, skill.Name);
        return skill;
    }

    public async Task<Skill?> UpdateSkillAsync(int id, UpdateSkillRequest request)
    {
        _logger.LogInformation("Updating skill {SkillId}", id);

        var skill = await _context.Skills.FindAsync(id);
        if (skill == null)
        {
            return null;
        }

        // Check if another skill with the same name exists
        var existingSkill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id != id && s.Name.ToLower() == request.Name.ToLower());

        if (existingSkill != null)
        {
            throw new ArgumentException($"Another skill with name '{request.Name}' already exists");
        }

        skill.Name = request.Name;
        skill.Description = request.Description;
        skill.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully updated skill {SkillId}: {SkillName}", skill.Id, skill.Name);
        return skill;
    }

    public async Task<bool> DeleteSkillAsync(int id)
    {
        _logger.LogInformation("Deleting skill {SkillId}", id);

        var skill = await _context.Skills.FindAsync(id);
        if (skill == null)
        {
            return false;
        }

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted skill {SkillId}", id);
        return true;
    }

    public async Task<bool> ToggleSkillActiveAsync(int id)
    {
        _logger.LogInformation("Toggling active status for skill {SkillId}", id);

        var skill = await _context.Skills.FindAsync(id);
        if (skill == null)
        {
            return false;
        }

        skill.IsActive = !skill.IsActive;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully toggled skill {SkillId} to {Status}", id, skill.IsActive ? "active" : "inactive");
        return true;
    }

    public async Task<List<string>> ResolveSkillNamesAsync(List<int> skillIds)
    {
        if (!skillIds.Any())
        {
            return new List<string>();
        }

        var skillNames = await _context.Skills
            .Where(s => skillIds.Contains(s.Id) && s.IsActive)
            .Select(s => s.Name)
            .ToListAsync();

        _logger.LogInformation("Resolved {Count} skill names from {InputCount} skill IDs",
            skillNames.Count, skillIds.Count);

        return skillNames;
    }
    
    public async Task<bool> IsServiceHealthyAsync()
    {
        try
        {
            // Check if the database is reachable
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection check failed");
            return false;
        }
    }
} 