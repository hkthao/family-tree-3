using backend.Domain.Common;
using backend.Domain.Enums;
using System.Text.Json;

namespace backend.Domain.Entities;

/// <summary>
/// Represents an AI-generated biography for a family member.
/// </summary>
public class AIBiography : BaseAuditableEntity
{
    /// <summary>
    /// The ID of the member for whom the biography was generated.
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Navigation property for the Member.
    /// </summary>
    public Member Member { get; set; } = null!;

    /// <summary>
    /// The style of the generated biography (e.g., emotional, historical).
    /// </summary>
    public BiographyStyle Style { get; set; }

    /// <summary>
    /// The content of the AI-generated biography.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// The AI provider used to generate the biography.
    /// </summary>
    public AIProviderType Provider { get; set; }

    /// <summary>
    /// The original prompt entered by the user.
    /// </summary>
    public string UserPrompt { get; set; } = null!;

    /// <summary>
    /// Indicates if the biography was generated using data combined from the database.
    /// </summary>
    public bool GeneratedFromDB { get; set; }

    /// <summary>
    /// The number of tokens used for generation.
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Optional metadata in JSON format for storing additional details about the generation process.
    /// </summary>
    public JsonDocument? Metadata { get; set; }
}
