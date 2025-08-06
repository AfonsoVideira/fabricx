using InteractionApi.Models;
using InteractionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly ILogger<SkillController> _logger;

    public SkillController(ISkillService skillService, ILogger<SkillController> logger)
    {
        _skillService = skillService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Skill>>>> GetAllSkills([FromQuery] bool activeOnly = false)
    {
        try
        {
            var skills = activeOnly 
                ? await _skillService.GetActiveSkillsAsync() 
                : await _skillService.GetAllSkillsAsync();

            return Ok(new ApiResponse<IEnumerable<Skill>>
            {
                Success = true,
                Message = $"Retrieved {skills.Count()} skills",
                Data = skills
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skills");
            return StatusCode(500, new ApiResponse<IEnumerable<Skill>>
            {
                Success = false,
                Message = "Internal server error occurred while retrieving skills",
                Data = null
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Skill>>> GetSkill(int id)
    {
        try
        {
            var skill = await _skillService.GetSkillByIdAsync(id);
            if (skill == null)
            {
                return NotFound(new ApiResponse<Skill>
                {
                    Success = false,
                    Message = $"Skill with ID {id} not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<Skill>
            {
                Success = true,
                Message = "Skill retrieved successfully",
                Data = skill
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill {SkillId}", id);
            return StatusCode(500, new ApiResponse<Skill>
            {
                Success = false,
                Message = "Internal server error occurred while retrieving skill",
                Data = null
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Skill>>> CreateSkill([FromBody] CreateSkillRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<Skill>
            {
                Success = false,
                Message = "Invalid request data",
                Data = null
            });
        }

        try
        {
            _logger.LogInformation("Admin creating new skill: {SkillName}", request.Name);

            var skill = await _skillService.CreateSkillAsync(request);

            return CreatedAtAction(
                nameof(GetSkill),
                new { id = skill.Id },
                new ApiResponse<Skill>
                {
                    Success = true,
                    Message = "Skill created successfully",
                    Data = skill
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<Skill>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skill");
            return StatusCode(500, new ApiResponse<Skill>
            {
                Success = false,
                Message = "Internal server error occurred while creating skill",
                Data = null
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Skill>>> UpdateSkill(int id, [FromBody] UpdateSkillRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<Skill>
            {
                Success = false,
                Message = "Invalid request data",
                Data = null
            });
        }

        try
        {
            _logger.LogInformation("Admin updating skill {SkillId}", id);

            var skill = await _skillService.UpdateSkillAsync(id, request);
            if (skill == null)
            {
                return NotFound(new ApiResponse<Skill>
                {
                    Success = false,
                    Message = $"Skill with ID {id} not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<Skill>
            {
                Success = true,
                Message = "Skill updated successfully",
                Data = skill
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<Skill>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill {SkillId}", id);
            return StatusCode(500, new ApiResponse<Skill>
            {
                Success = false,
                Message = "Internal server error occurred while updating skill",
                Data = null
            });
        }
    }

    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleSkillActive(int id)
    {
        try
        {
            _logger.LogInformation("Admin toggling active status for skill {SkillId}", id);

            var success = await _skillService.ToggleSkillActiveAsync(id);
            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Skill with ID {id} not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Skill status toggled successfully",
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling skill {SkillId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while toggling skill status",
                Data = null
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSkill(int id)
    {
        try
        {
            _logger.LogInformation("Admin deleting skill {SkillId}", id);

            var success = await _skillService.DeleteSkillAsync(id);
            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Skill with ID {id} not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Skill deleted successfully",
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skill {SkillId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error occurred while deleting skill",
                Data = null
            });
        }
    }
} 