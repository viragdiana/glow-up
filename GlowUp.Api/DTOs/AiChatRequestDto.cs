using System.ComponentModel.DataAnnotations;

namespace GlowUp.Api.DTOs;

/// <summary>Request body for POST /api/ai-chat.</summary>
public class AiChatRequestDto
{
    [Required]
    [MinLength(1)]
    public string Question { get; set; } = string.Empty;
}
