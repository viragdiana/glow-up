namespace GlowUp.Api.DTOs;

/// <summary>
/// A preview of the context that will later be sent to an AI model.
/// For now it simply bundles the profile, its sections, and a generated
/// plain-text summary. No external AI call is made.
/// </summary>
public class AiContextDto
{
    public ProfileDto Profile { get; set; } = new();
    public List<ProfileSectionDto> Sections { get; set; } = new();

    /// <summary>Plain-text summary combining all profile and section data.</summary>
    public string GeneratedSummary { get; set; } = string.Empty;
}
