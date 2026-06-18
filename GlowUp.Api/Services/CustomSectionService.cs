using GlowUp.Api.Data;
using GlowUp.Api.DTOs;
using GlowUp.Api.Enums;
using GlowUp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.Api.Services;

public class CustomSectionService : ICustomSectionService
{
    private readonly AppDbContext _db;
    private readonly IProfileService _profileService;

    public CustomSectionService(AppDbContext db, IProfileService profileService)
    {
        _db = db;
        _profileService = profileService;
    }

    public async Task<List<ProfileSectionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);

        var sections = await _db.ProfileSections
            .AsNoTracking()
            .Where(s => s.ProfileId == profile.Id && s.SectionType == SectionType.Custom)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return sections.Select(s => s.ToDto()).ToList();
    }

    public async Task<ProfileSectionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var section = await FindCustomAsync(id, tracking: false, cancellationToken);
        return section?.ToDto();
    }

    public async Task<ProfileSectionDto> CreateAsync(CustomSectionInputDto dto, CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var section = new ProfileSection
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            SectionType = SectionType.Custom,
            Title = dto.Title,
            Summary = dto.Summary,
            Notes = dto.Notes,
            ConfidenceLevel = dto.ConfidenceLevel,
            DataJson = NormalizeDataJson(dto.DataJson),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.ProfileSections.Add(section);
        await _db.SaveChangesAsync(cancellationToken);

        return section.ToDto();
    }

    public async Task<ProfileSectionDto?> UpdateAsync(Guid id, CustomSectionInputDto dto, CancellationToken cancellationToken = default)
    {
        var section = await FindCustomAsync(id, tracking: true, cancellationToken);
        if (section is null)
            return null;

        section.Title = dto.Title;
        section.Summary = dto.Summary;
        section.Notes = dto.Notes;
        section.ConfidenceLevel = dto.ConfidenceLevel;
        section.DataJson = NormalizeDataJson(dto.DataJson);
        section.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return section.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var section = await FindCustomAsync(id, tracking: true, cancellationToken);
        if (section is null)
            return false;

        _db.ProfileSections.Remove(section);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Looks up a section by id, scoped to the main profile and to Custom only,
    /// so fixed sections can never be reached through the custom endpoints.
    /// </summary>
    private async Task<ProfileSection?> FindCustomAsync(Guid id, bool tracking, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);

        var query = _db.ProfileSections.AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(
            s => s.Id == id && s.ProfileId == profile.Id && s.SectionType == SectionType.Custom,
            cancellationToken);
    }

    private static string NormalizeDataJson(string? dataJson) =>
        string.IsNullOrWhiteSpace(dataJson) ? "{}" : dataJson;
}
