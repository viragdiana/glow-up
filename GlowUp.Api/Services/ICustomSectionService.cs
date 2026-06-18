using GlowUp.Api.DTOs;

namespace GlowUp.Api.Services;

/// <summary>
/// Manages user-created (Custom) sections by id. Fixed sections continue to be
/// handled by <see cref="ISectionService"/>.
/// </summary>
public interface ICustomSectionService
{
    Task<List<ProfileSectionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProfileSectionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProfileSectionDto> CreateAsync(CustomSectionInputDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns the updated section, or null if no custom section with that id exists.</summary>
    Task<ProfileSectionDto?> UpdateAsync(Guid id, CustomSectionInputDto dto, CancellationToken cancellationToken = default);

    /// <summary>Returns false if no custom section with that id exists.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
