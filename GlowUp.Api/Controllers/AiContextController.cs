using GlowUp.Api.DTOs;
using GlowUp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlowUp.Api.Controllers;

[ApiController]
[Route("api/ai-context")]
public class AiContextController : ControllerBase
{
    private readonly IAiContextService _aiContextService;

    public AiContextController(IAiContextService aiContextService)
    {
        _aiContextService = aiContextService;
    }

    /// <summary>
    /// Returns the profile, all sections, and a generated plain-text summary.
    /// This is a preview of the context that will later be sent to an AI model;
    /// no external AI call is made yet.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AiContextDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AiContextDto>> GetAiContext(CancellationToken cancellationToken)
    {
        var context = await _aiContextService.GetAiContextAsync(cancellationToken);
        return Ok(context);
    }
}
