namespace backend.Application.Common.Models;

public class EmbeddingWebhookDto
{
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string ActionType { get; set; } = null!;
    public object EntityData { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
}
