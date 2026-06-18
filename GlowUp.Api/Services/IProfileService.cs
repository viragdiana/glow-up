using GlowUp.Api.DTOs;

namespace GlowUp.Api.Services;

public interface IProfileService
{
    /// <summary>Returns the main profile, creating a default one if none exists.</summary>
    Task<ProfileDto> GetOrCreateMainProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the main profile (creating it first if necessary).</summary>
    Task<ProfileDto> UpdateMainProfileAsync(UpdateProfileDto dto, CancellationToken cancellationToken = default);
}
