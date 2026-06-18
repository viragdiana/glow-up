using GlowUp.Api.DTOs;
using GlowUp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlowUp.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>Returns the main profile, creating a default one if none exists.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);
        return Ok(profile);
    }

    /// <summary>Updates the main profile.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(
        [FromBody] UpdateProfileDto dto,
        CancellationToken cancellationToken)
    {
        var profile = await _profileService.UpdateMainProfileAsync(dto, cancellationToken);
        return Ok(profile);
    }
}
