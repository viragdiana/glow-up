using GlowUp.Api.DTOs;
using GlowUp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlowUp.Api.Controllers;

[ApiController]
[Route("api/ai-chat")]
public class AiChatController : ControllerBase
{
    private readonly IAiChatService _aiChatService;

    public AiChatController(IAiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    /// <summary>
    /// Answers a question about the user's saved profile data.
    /// Currently backed by a mock AI provider — no external AI call is made.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AiChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AiChatResponseDto>> Ask(
        [FromBody] AiChatRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question must not be empty." });

        var response = await _aiChatService.AskAsync(request, cancellationToken);
        return Ok(response);
    }
}
