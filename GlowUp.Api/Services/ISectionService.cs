using GlowUp.Api.DTOs;
using GlowUp.Api.Enums;

namespace GlowUp.Api.Services;

public interface ISectionService
{
    /// <summary>Returns all sections belonging to the main profile.</summary>
    Task<List<ProfileSectionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single section by its type, or null if it does not exist yet.</summary>
    Task<ProfileSectionDto?> GetByTypeAsync(SectionType sectionType, CancellationToken cancellationToken = default);

    /// <summary>Creates the section if missing, otherwise updates it (upsert by type).</summary>
    Task<ProfileSectionDto> UpsertByTypeAsync(SectionType sectionType, UpdateProfileSectionDto dto, CancellationToken cancellationToken = default);
}
