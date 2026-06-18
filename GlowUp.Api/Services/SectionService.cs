using GlowUp.Api.Data;
using GlowUp.Api.DTOs;
using GlowUp.Api.Enums;
using GlowUp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.Api.Services;

public class SectionService : ISectionService
{
    private readonly AppDbContext _db;
    private readonly IProfileService _profileService;

    public SectionService(AppDbContext db, IProfileService profileService)
    {
        _db = db;
        _profileService = profileService;
    }

    public async Task<List<ProfileSectionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);

        var sections = await _db.ProfileSections
            .AsNoTracking()
            .Where(s => s.ProfileId == profile.Id)
            .OrderBy(s => s.SectionType)
            .ToListAsync(cancellationToken);

        return sections.Select(s => s.ToDto()).ToList();
    }

    public async Task<ProfileSectionDto?> GetByTypeAsync(SectionType sectionType, CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);

        var section = await _db.ProfileSections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProfileId == profile.Id && s.SectionType == sectionType, cancellationToken);

        return section?.ToDto();
    }

    public async Task<ProfileSectionDto> UpsertByTypeAsync(SectionType sectionType, UpdateProfileSectionDto dto, CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);

        var section = await _db.ProfileSections
            .FirstOrDefaultAsync(s => s.ProfileId == profile.Id && s.SectionType == sectionType, cancellationToken);

        var now = DateTime.UtcNow;

        if (section is null)
        {
            section = new ProfileSection
            {
                Id = Guid.NewGuid(),
                ProfileId = profile.Id,
                SectionType = sectionType,
                CreatedAt = now
            };
            _db.ProfileSections.Add(section);
        }

        section.Title = dto.Title;
        section.Summary = dto.Summary;
        section.Notes = dto.Notes;
        section.ConfidenceLevel = dto.ConfidenceLevel;
        section.DataJson = string.IsNullOrWhiteSpace(dto.DataJson) ? "{}" : dto.DataJson;
        section.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);

        return section.ToDto();
    }
}
