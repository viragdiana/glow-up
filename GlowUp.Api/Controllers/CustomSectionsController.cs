using GlowUp.Api.DTOs;
using GlowUp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlowUp.Api.Controllers;

[ApiController]
[Route("api/custom-sections")]
public class CustomSectionsController : ControllerBase
{
    private readonly ICustomSectionService _customSectionService;

    public CustomSectionsController(ICustomSectionService customSectionService)
    {
        _customSectionService = customSectionService;
    }

    /// <summary>Returns all custom sections for the main profile.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProfileSectionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProfileSectionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var sections = await _customSectionService.GetAllAsync(cancellationToken);
        return Ok(sections);
    }

    /// <summary>Returns a single custom section by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProfileSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileSectionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var section = await _customSectionService.GetByIdAsync(id, cancellationToken);
        return section is null ? NotFound() : Ok(section);
    }

    /// <summary>Creates a new custom section.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProfileSectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProfileSectionDto>> Create(
        [FromBody] CustomSectionInputDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _customSectionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing custom section.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProfileSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileSectionDto>> Update(
        Guid id,
        [FromBody] CustomSectionInputDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _customSectionService.UpdateAsync(id, dto, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Deletes a custom section.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _customSectionService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
