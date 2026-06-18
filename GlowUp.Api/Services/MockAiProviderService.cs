using System.Text;

namespace GlowUp.Api.Services;

/// <summary>
/// A stand-in AI provider that produces realistic, context-grounded answers
/// without calling any external API. It inspects the question for intent and
/// weaves in the supplied profile context. Swap this out for a real provider
/// later by registering a different <see cref="IAiProviderService"/>.
/// </summary>
public class MockAiProviderService : IAiProviderService
{
    public async Task<string> GenerateAnswerAsync(
        string question,
        string contextSummary,
        CancellationToken cancellationToken = default)
    {
        // Simulate provider latency so the frontend loading state is exercised.
        await Task.Delay(450, cancellationToken);

        var hasContext = !string.IsNullOrWhiteSpace(contextSummary)
                         && !contextSummary.Contains("No self-knowledge sections have been filled in yet.");

        var sb = new StringBuilder();

        var intent = DetectIntent(question);

        sb.AppendLine(intent switch
        {
            Intent.Study =>
                "Here's how your profile points toward an effective study approach:",
            Intent.Career =>
                "Based on your saved profile, here are career directions that tend to fit:",
            Intent.Style =>
                "Combining your style and personality data, here's a personal-style read:",
            Intent.Growth =>
                "Here are personal-growth focus areas drawn from your profile:",
            Intent.Summarize =>
                "Here is a summary of your full self-knowledge profile:",
            _ =>
                "Here's what your saved profile suggests in response to your question:",
        });
        sb.AppendLine();

        if (!hasContext)
        {
            sb.AppendLine(
                "Your profile doesn't have any self-knowledge sections filled in yet, so this answer is general. " +
                "Add details under MBTI, Kibbe, Astrology, Human Design, or Big Five and ask again for a tailored response.");
            return sb.ToString().TrimEnd();
        }

        // Ground the answer in the actual context. The mock simply reflects the
        // saved data back in an intent-appropriate framing.
        sb.AppendLine(intent switch
        {
            Intent.Study =>
                "- Lean into the strengths captured in your MBTI and Big Five notes, and structure study sessions to match them.\n" +
                "- Where your notes flag challenges, add scaffolding (checklists, timers, study partners) to compensate.\n" +
                "- Use your Openness and Conscientiousness signals to balance exploration with consistent routine.",
            Intent.Career =>
                "- Favor roles that reward your documented strengths and core type.\n" +
                "- Watch for environments that amplify the challenges noted in your profile.\n" +
                "- Treat your Human Design strategy/authority notes as a decision-making filter for opportunities.",
            Intent.Style =>
                "- Your Kibbe body type and essence notes set the silhouette and lines that suit you.\n" +
                "- Let your personality (MBTI / Big Five) guide how bold or understated those choices feel.\n" +
                "- Apply your style recommendations and respect the 'avoid' notes you saved.",
            Intent.Growth =>
                "- Double down on the strengths you recorded — they're your fastest leverage.\n" +
                "- Pick one challenge from your notes and define a single concrete habit around it.\n" +
                "- Revisit your astrology/Human Design notes for themes that keep recurring across sections.",
            _ =>
                "I've drawn the points above directly from the sections you've saved.",
        });
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine("Grounded in your saved profile:");
        sb.AppendLine();
        sb.Append(contextSummary.Trim());

        return sb.ToString().TrimEnd();
    }

    private enum Intent
    {
        Generic,
        Summarize,
        Study,
        Career,
        Style,
        Growth,
    }

    private static Intent DetectIntent(string question)
    {
        var q = question.ToLowerInvariant();

        if (Contains(q, "summar", "overview", "whole", "full profile", "everything"))
            return Intent.Summarize;
        if (Contains(q, "study", "learn", "school", "exam", "focus while"))
            return Intent.Study;
        if (Contains(q, "career", "job", "work", "profession", "vocation"))
            return Intent.Career;
        if (Contains(q, "style", "fashion", "outfit", "wardrobe", "clothes", "kibbe"))
            return Intent.Style;
        if (Contains(q, "grow", "improve", "better", "develop", "self-improvement"))
            return Intent.Growth;

        return Intent.Generic;
    }

    private static bool Contains(string haystack, params string[] needles)
    {
        foreach (var needle in needles)
        {
            if (haystack.Contains(needle, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
