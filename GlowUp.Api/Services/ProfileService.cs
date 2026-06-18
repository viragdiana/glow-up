using GlowUp.Api.Data;
using GlowUp.Api.DTOs;
using GlowUp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.Api.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;

    public ProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProfileDto> GetOrCreateMainProfileAsync(CancellationToken cancellationToken = default)
    {
        var profile = await GetMainProfileEntityAsync(cancellationToken)
                      ?? await CreateMainProfileAsync(cancellationToken);

        return MapToDto(profile);
    }

    public async Task<ProfileDto> UpdateMainProfileAsync(UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var profile = await GetMainProfileEntityAsync(cancellationToken)
                      ?? await CreateMainProfileAsync(cancellationToken);

        profile.DisplayName = dto.DisplayName;
        profile.Bio = dto.Bio;
        profile.AvatarUrl = dto.AvatarUrl;
        profile.BirthDate = dto.BirthDate;
        profile.Location = dto.Location;
        profile.GeneralNotes = dto.GeneralNotes;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return MapToDto(profile);
    }

    /// <summary>The MVP has a single profile, so we return the earliest-created one.</summary>
    private Task<Profile?> GetMainProfileEntityAsync(CancellationToken cancellationToken) =>
        _db.Profiles
           .OrderBy(p => p.CreatedAt)
           .FirstOrDefaultAsync(cancellationToken);

    private async Task<Profile> CreateMainProfileAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            DisplayName = "My Profile",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);

        return profile;
    }

    private static ProfileDto MapToDto(Profile p) => new()
    {
        Id = p.Id,
        DisplayName = p.DisplayName,
        Bio = p.Bio,
        AvatarUrl = p.AvatarUrl,
        BirthDate = p.BirthDate,
        Location = p.Location,
        GeneralNotes = p.GeneralNotes,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
