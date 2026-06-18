using GlowUp.Api.DTOs;

namespace GlowUp.Api.Services;

public class AiChatService : IAiChatService
{
    private readonly IAiContextService _aiContextService;
    private readonly IAiProviderService _aiProvider;

    public AiChatService(IAiContextService aiContextService, IAiProviderService aiProvider)
    {
        _aiContextService = aiContextService;
        _aiProvider = aiProvider;
    }

    public async Task<AiChatResponseDto> AskAsync(AiChatRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            throw new ArgumentException("Question must not be empty.", nameof(request));

        // Reuse the existing context builder so the chat is grounded in the
        // same summary the AI Context Preview shows.
        var context = await _aiContextService.GetAiContextAsync(cancellationToken);

        var answer = await _aiProvider.GenerateAnswerAsync(
            request.Question.Trim(),
            context.GeneratedSummary,
            cancellationToken);

        return new AiChatResponseDto
        {
            Answer = answer,
            UsedContextSummary = context.GeneratedSummary,
            CreatedAt = DateTime.UtcNow
        };
    }
}
