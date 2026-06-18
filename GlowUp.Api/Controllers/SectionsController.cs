using GlowUp.Api.DTOs;
using GlowUp.Api.Enums;
using GlowUp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlowUp.Api.Controllers;

[ApiController]
[Route("api/sections")]
public class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    /// <summary>Returns all sections for the main profile.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProfileSectionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProfileSectionDto>>> GetSections(CancellationToken cancellationToken)
    {
        var sections = await _sectionService.GetAllAsync(cancellationToken);
        return Ok(sections);
    }

    /// <summary>Returns a single section by its type.</summary>
    [HttpGet("{sectionType}")]
    [ProducesResponseType(typeof(ProfileSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileSectionDto>> GetSection(
        SectionType sectionType,
        CancellationToken cancellationToken)
    {
        var section = await _sectionService.GetByTypeAsync(sectionType, cancellationToken);
        return section is null ? NotFound() : Ok(section);
    }

    /// <summary>Creates or updates a section by its type (upsert).</summary>
    [HttpPut("{sectionType}")]
    [ProducesResponseType(typeof(ProfileSectionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileSectionDto>> UpsertSection(
        SectionType sectionType,
        [FromBody] UpdateProfileSectionDto dto,
        CancellationToken cancellationToken)
    {
        var section = await _sectionService.UpsertByTypeAsync(sectionType, dto, cancellationToken);
        return Ok(section);
    }
}
