namespace GlowUp.Api.DTOs;

/// <summary>Read model returned for the main profile.</summary>
public class ProfileDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Location { get; set; }
    public string? GeneralNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
