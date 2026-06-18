using GlowUp.Api.DTOs;

namespace GlowUp.Api.Services;

public interface IAiChatService
{
    /// <summary>
    /// Answers a question about the user's profile: loads the profile context,
    /// hands it plus the question to the AI provider, and returns the answer.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the question is empty.</exception>
    Task<AiChatResponseDto> AskAsync(AiChatRequestDto request, CancellationToken cancellationToken = default);
}
