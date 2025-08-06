using InteractionApi.Models;

namespace InteractionApi.Services;

public interface ISkillService
{
    Task<IEnumerable<Skill>> GetAllSkillsAsync();
    Task<IEnumerable<Skill>> GetActiveSkillsAsync();
    Task<Skill?> GetSkillByIdAsync(int id);
    Task<Skill> CreateSkillAsync(CreateSkillRequest request);
    Task<Skill?> UpdateSkillAsync(int id, UpdateSkillRequest request);
    Task<bool> DeleteSkillAsync(int id);
    Task<bool> ToggleSkillActiveAsync(int id);
    Task<List<string>> ResolveSkillNamesAsync(List<int> skillIds);
    Task<bool> IsServiceHealthyAsync();
} 