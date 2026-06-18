using System.Text.Json;
using GlowUp.Api.Data;
using GlowUp.Api.Enums;
using GlowUp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.Api.Services;

/// <summary>
/// Seeds portfolio-friendly sample data for the public demo so visitors see a
/// filled-in profile rather than an empty app (and never the developer's private
/// data). Runs only when <c>Demo:Seed</c> is enabled, and is idempotent: it does
/// nothing if the database already contains any sections.
/// </summary>
public class DemoDataSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(AppDbContext db, ILogger<DemoDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Idempotent guard: if any section already exists, assume the demo is set up.
        if (await _db.ProfileSections.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Demo seed skipped: sections already present.");
            return;
        }

        var now = DateTime.UtcNow;

        var profile = await _db.Profiles.OrderBy(p => p.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (profile is null)
        {
            profile = new Profile { Id = Guid.NewGuid(), CreatedAt = now };
            _db.Profiles.Add(profile);
        }

        profile.DisplayName = "Alex Demo";
        profile.Bio = "Sample profile showcasing the Glow Up self-knowledge dashboard. " +
                      "All data here is fictional demo content.";
        profile.Location = "Lisbon, Portugal";
        profile.GeneralNotes = "Curious, creative, and building a calmer relationship with productivity.";
        profile.UpdatedAt = now;

        var sections = new List<ProfileSection>
        {
            MakeSection(profile.Id, SectionType.Mbti, "INFP – The Mediator", now,
                summary: "Idealistic, values-driven, and imaginative.",
                confidence: 80,
                data: new
                {
                    type = "INFP",
                    cognitiveFunctionsNotes = "Fi dominant, Ne auxiliary — led by inner values and possibilities.",
                    strengths = "Empathy, creativity, big-picture vision.",
                    challenges = "Follow-through on routine tasks; over-idealizing."
                }),

            MakeSection(profile.Id, SectionType.Kibbe, "Soft Natural", now,
                summary: "Yang-balanced-with-soft, relaxed and approachable lines.",
                confidence: 60,
                data: new
                {
                    bodyType = "Soft Natural",
                    essenceNotes = "Warm, easygoing, slightly boho.",
                    styleRecommendations = "Soft tailoring, draped fabrics, relaxed silhouettes.",
                    avoidNotes = "Overly stiff or sharp geometric cuts."
                }),

            MakeSection(profile.Id, SectionType.Astrology, "Pisces Sun", now,
                summary: "Water-heavy chart with a curious, communicative streak.",
                confidence: 50,
                data: new
                {
                    sunSign = "Pisces",
                    moonSign = "Cancer",
                    risingSign = "Libra",
                    venusSign = "Aquarius",
                    marsSign = "Scorpio",
                    notes = "Dreamy but driven; values harmony and depth."
                }),

            MakeSection(profile.Id, SectionType.HumanDesign, "Generator", now,
                summary: "Responds to life with sustainable, satisfying energy.",
                confidence: 55,
                data: new
                {
                    type = "Generator",
                    strategy = "Wait to respond",
                    authority = "Sacral",
                    profile = "3/5",
                    centersNotes = "Defined Sacral and Throat; open Head and Ajna."
                }),

            MakeSection(profile.Id, SectionType.BigFive, "Big Five Snapshot", now,
                summary: "High Openness, moderate Conscientiousness, low-ish Neuroticism.",
                confidence: 75,
                data: new
                {
                    openness = 88,
                    conscientiousness = 62,
                    extraversion = 45,
                    agreeableness = 78,
                    neuroticism = 40,
                    interpretationNotes = "Creative and warm; benefits from light structure."
                }),

            MakeSection(profile.Id, SectionType.Custom, "Enneagram", now,
                summary: "Type 4w5 — introspective and individualistic.",
                confidence: 70,
                data: new
                {
                    Type = "4w5",
                    Tritype = "459",
                    CoreDesire = "Authentic identity",
                    CoreFear = "Having no significance"
                }),

            MakeSection(profile.Id, SectionType.Custom, "Love Languages", now,
                summary: "Quality Time and Words of Affirmation lead.",
                confidence: 65,
                data: new
                {
                    Primary = "Quality Time",
                    Secondary = "Words of Affirmation",
                    Notes = "Feels most connected through undistracted conversation."
                }),
        };

        _db.ProfileSections.AddRange(sections);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Demo data seeded: 1 profile, {Count} sections.", sections.Count);
    }

    private static ProfileSection MakeSection(
        Guid profileId, SectionType type, string title, DateTime now,
        string summary, int confidence, object data) => new()
    {
        Id = Guid.NewGuid(),
        ProfileId = profileId,
        SectionType = type,
        Title = title,
        Summary = summary,
        Notes = null,
        ConfidenceLevel = confidence,
        DataJson = JsonSerializer.Serialize(data),
        CreatedAt = now,
        UpdatedAt = now
    };
}
