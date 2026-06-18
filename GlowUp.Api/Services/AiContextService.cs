using System.Text;
using GlowUp.Api.DTOs;

namespace GlowUp.Api.Services;

public class AiContextService : IAiContextService
{
    private readonly IProfileService _profileService;
    private readonly ISectionService _sectionService;

    public AiContextService(IProfileService profileService, ISectionService sectionService)
    {
        _profileService = profileService;
        _sectionService = sectionService;
    }

    public async Task<AiContextDto> GetAiContextAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetOrCreateMainProfileAsync(cancellationToken);
        var sections = await _sectionService.GetAllAsync(cancellationToken);

        return new AiContextDto
        {
            Profile = profile,
            Sections = sections,
            GeneratedSummary = BuildSummary(profile, sections)
        };
    }

    /// <summary>
    /// Composes a human-readable summary of everything we know about the profile.
    /// This is the text that would later be handed to an AI model as context.
    /// </summary>
    private static string BuildSummary(ProfileDto profile, IReadOnlyList<ProfileSectionDto> sections)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Profile: {profile.DisplayName}");

        if (profile.BirthDate is { } birthDate)
            sb.AppendLine($"Birth date: {birthDate:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(profile.Location))
            sb.AppendLine($"Location: {profile.Location}");

        if (!string.IsNullOrWhiteSpace(profile.Bio))
            sb.AppendLine($"Bio: {profile.Bio}");

        if (!string.IsNullOrWhiteSpace(profile.GeneralNotes))
            sb.AppendLine($"General notes: {profile.GeneralNotes}");

        sb.AppendLine();

        if (sections.Count == 0)
        {
            sb.AppendLine("No self-knowledge sections have been filled in yet.");
            return sb.ToString().TrimEnd();
        }

        sb.AppendLine("Self-knowledge sections:");

        foreach (var section in sections)
        {
            sb.AppendLine();
            sb.AppendLine($"## {section.SectionType} — {section.Title}");

            if (section.ConfidenceLevel is { } confidence)
                sb.AppendLine($"Confidence: {confidence}%");

            if (!string.IsNullOrWhiteSpace(section.Summary))
                sb.AppendLine($"Summary: {section.Summary}");

            if (!string.IsNullOrWhiteSpace(section.Notes))
                sb.AppendLine($"Notes: {section.Notes}");

            if (!string.IsNullOrWhiteSpace(section.DataJson) && section.DataJson.Trim() != "{}")
                sb.AppendLine($"Data: {section.DataJson}");
        }

        return sb.ToString().TrimEnd();
    }
}
