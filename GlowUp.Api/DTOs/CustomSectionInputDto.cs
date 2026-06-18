using System.ComponentModel.DataAnnotations;

namespace GlowUp.Api.DTOs;

/// <summary>
/// Body for creating (POST) or updating (PUT) a custom section.
/// The section type is always Custom and is set by the server, so it is not
/// part of the body. Reads are returned as <see cref="ProfileSectionDto"/>.
/// </summary>
public class CustomSectionInputDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Notes { get; set; }

    [Range(0, 100)]
    public int? ConfidenceLevel { get; set; }

    /// <summary>Free-form key/value fields serialized as JSON. Defaults to an empty object.</summary>
    public string DataJson { get; set; } = "{}";
}
