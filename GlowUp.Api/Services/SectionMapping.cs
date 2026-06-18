using GlowUp.Api.DTOs;
using GlowUp.Api.Models;

namespace GlowUp.Api.Services;

/// <summary>Shared mapping from the <see cref="ProfileSection"/> entity to its DTO.</summary>
public static class SectionMapping
{
    public static ProfileSectionDto ToDto(this ProfileSection s) => new()
    {
        Id = s.Id,
        ProfileId = s.ProfileId,
        SectionType = s.SectionType,
        Title = s.Title,
        Summary = s.Summary,
        Notes = s.Notes,
        ConfidenceLevel = s.ConfidenceLevel,
        DataJson = s.DataJson,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
