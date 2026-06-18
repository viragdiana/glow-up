using GlowUp.Api.Enums;

namespace GlowUp.Api.DTOs;

/// <summary>Read model returned for a profile section.</summary>
public class ProfileSectionDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public SectionType SectionType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Notes { get; set; }
    public int? ConfidenceLevel { get; set; }
    public string DataJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
