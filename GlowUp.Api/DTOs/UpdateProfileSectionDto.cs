using System.ComponentModel.DataAnnotations;

namespace GlowUp.Api.DTOs;

/// <summary>
/// Write model used to create or update a section by its type
/// (PUT /api/sections/{sectionType}). The section type comes from the route,
/// so it is not part of the body.
/// </summary>
public class UpdateProfileSectionDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Notes { get; set; }

    [Range(0, 100)]
    public int? ConfidenceLevel { get; set; }

    /// <summary>Structured payload serialized as JSON. Defaults to an empty object.</summary>
    public string DataJson { get; set; } = "{}";
}
