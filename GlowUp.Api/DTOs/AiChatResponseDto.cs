namespace GlowUp.Api.DTOs;

/// <summary>Response body for POST /api/ai-chat.</summary>
public class AiChatResponseDto
{
    /// <summary>The (currently mocked) answer generated from the saved profile data.</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>The plain-text profile context the answer was grounded in.</summary>
    public string UsedContextSummary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
