using Google.GenAI;

namespace GlowUp.Api.Services;

/// <summary>
/// Real AI provider backed by Google Gemini via the official Google.GenAI SDK.
/// The API key and model are read from configuration (user-secrets in local dev) —
/// never hardcoded. If anything goes wrong (missing key, network/SDK error), it
/// logs the real error server-side and quietly falls back to the mock provider so
/// the app keeps working and no secret or raw exception ever reaches the frontend.
/// </summary>
public class GeminiAiProviderService : IAiProviderService
{
    private const string DefaultModel = "gemini-2.5-flash";

    private const string SystemInstructions =
        "You are Glow Up's personal insight assistant. Use only the saved profile context " +
        "provided by the app. Give clear, practical, personalized answers. Treat personality, " +
        "astrology, Human Design, Kibbe, and similar systems as reflective self-knowledge " +
        "frameworks, not scientific certainty. Do not make medical, legal, or financial claims. " +
        "If profile data is missing, say what is missing and answer carefully.";

    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiProviderService> _logger;
    private readonly MockAiProviderService _fallback;

    public GeminiAiProviderService(
        IConfiguration configuration,
        ILogger<GeminiAiProviderService> logger,
        MockAiProviderService fallback)
    {
        _configuration = configuration;
        _logger = logger;
        _fallback = fallback;
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        string contextSummary,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["AI:Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini provider selected but no API key configured; falling back to mock.");
            return await _fallback.GenerateAnswerAsync(question, contextSummary, cancellationToken);
        }

        var model = _configuration["AI:Gemini:Model"];
        if (string.IsNullOrWhiteSpace(model))
            model = DefaultModel;

        var prompt = BuildPrompt(question, contextSummary);

        try
        {
            using var client = new Client(apiKey: apiKey);

            var response = await client.Models.GenerateContentAsync(
                model: model,
                contents: prompt,
                config: null,
                cancellationToken: cancellationToken);

            var answer = response.Text;
            if (string.IsNullOrWhiteSpace(answer))
            {
                _logger.LogWarning("Gemini returned an empty response; falling back to mock.");
                return await _fallback.GenerateAnswerAsync(question, contextSummary, cancellationToken);
            }

            return answer.Trim();
        }
        catch (Exception ex)
        {
            // Log full detail server-side; never surface raw exception/secret to the caller.
            _logger.LogError(ex, "Gemini request failed; falling back to mock provider.");
            return await _fallback.GenerateAnswerAsync(question, contextSummary, cancellationToken);
        }
    }

    private static string BuildPrompt(string question, string contextSummary)
    {
        var context = string.IsNullOrWhiteSpace(contextSummary)
            ? "(No saved profile context is available.)"
            : contextSummary.Trim();

        return $"""
            {SystemInstructions}

            --- SAVED PROFILE CONTEXT ---
            {context}

            --- USER QUESTION ---
            {question.Trim()}
            """;
    }
}
