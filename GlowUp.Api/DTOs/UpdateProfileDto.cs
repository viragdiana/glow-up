using System.ComponentModel.DataAnnotations;

namespace GlowUp.Api.DTOs;

/// <summary>Write model used to update the main profile (PUT /api/profile).</summary>
public class UpdateProfileDto
{
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }

    public DateOnly? BirthDate { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public string? GeneralNotes { get; set; }
}
