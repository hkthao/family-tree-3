using System.Text.Json;
using backend.Domain.Enums;

namespace backend.Application.AI.Common;

public class AIBiographyDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public BiographyStyle Style { get; set; }
    public string Content { get; set; } = null!;
    public AIProviderType Provider { get; set; }
    public string UserPrompt { get; set; } = null!;
    public bool GeneratedFromDB { get; set; }
    public int TokensUsed { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime Created { get; set; }
}
