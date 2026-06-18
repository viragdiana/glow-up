namespace GlowUp.Api.Enums;

/// <summary>
/// The kind of self-knowledge data a <see cref="Models.ProfileSection"/> holds.
/// Stored as a string in the database for readability and forward compatibility.
/// </summary>
public enum SectionType
{
    Mbti,
    Kibbe,
    Astrology,
    HumanDesign,
    BigFive,
    Custom
}
