using System.ComponentModel.DataAnnotations;
using GlowUp.Api.Enums;

namespace GlowUp.Api.Models;

/// <summary>
/// A single self-knowledge section (MBTI, Kibbe, Astrology, etc.) attached to a profile.
/// Structured/free-form section data is stored in <see cref="DataJson"/>.
/// </summary>
public class ProfileSection
{
    public Guid Id { get; set; }

    public Guid ProfileId { get; set; }

    public SectionType SectionType { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Notes { get; set; }

    /// <summary>Optional self-reported confidence in this section's data (e.g. 0–100).</summary>
    public int? ConfidenceLevel { get; set; }

    /// <summary>Free-form structured payload serialized as JSON. Defaults to an empty object.</summary>
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>The owning profile. Ignored during serialization to avoid cycles.</summary>
    public Profile? Profile { get; set; }
}
