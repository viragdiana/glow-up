using GlowUp.Api.DTOs;

namespace GlowUp.Api.Services;

public interface IAiContextService
{
    /// <summary>
    /// Builds the AI context preview: the profile, all sections, and a generated
    /// plain-text summary. Does not call any external AI service yet.
    /// </summary>
    Task<AiContextDto> GetAiContextAsync(CancellationToken cancellationToken = default);
}
