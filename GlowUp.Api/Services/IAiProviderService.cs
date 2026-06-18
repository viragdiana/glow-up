namespace GlowUp.Api.Services;

/// <summary>
/// Abstraction over an AI text provider. Today this is backed by
/// <see cref="MockAiProviderService"/>; later it can wrap a real model API
/// without changing any callers.
/// </summary>
public interface IAiProviderService
{
    /// <summary>
    /// Generates an answer to <paramref name="question"/> grounded in the supplied
    /// <paramref name="contextSummary"/> (the user's saved profile data).
    /// </summary>
    Task<string> GenerateAnswerAsync(
        string question,
        string contextSummary,
        CancellationToken cancellationToken = default);
}
