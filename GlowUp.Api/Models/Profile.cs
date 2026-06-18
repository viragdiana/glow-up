using System.ComponentModel.DataAnnotations;

namespace GlowUp.Api.Models;

/// <summary>
/// The main user profile. For the MVP there is a single profile per database.
/// </summary>
public class Profile
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }

    public DateOnly? BirthDate { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public string? GeneralNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>All self-knowledge sections belonging to this profile.</summary>
    public List<ProfileSection> Sections { get; set; } = new();
}
