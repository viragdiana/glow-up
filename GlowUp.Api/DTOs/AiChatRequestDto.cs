using System.ComponentModel.DataAnnotations;

namespace GlowUp.Api.DTOs;

/// <summary>Request body for POST /api/ai-chat.</summary>
public class AiChatRequestDto
{
    /// <summary>Upper bound on question length to protect the public demo.</summary>
    public const int MaxQuestionLength = 1000;

    [Required]
    [MinLength(1)]
    [MaxLength(MaxQuestionLength)]
    public string Question { get; set; } = string.Empty;
}
