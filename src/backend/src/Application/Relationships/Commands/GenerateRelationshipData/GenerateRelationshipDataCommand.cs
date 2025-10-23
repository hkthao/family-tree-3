using backend.Application.Common.Models;

namespace backend.Application.Relationships.Commands.GenerateRelationshipData;

public record GenerateRelationshipDataCommand(string Prompt) : IRequest<Result<List<AIRelationshipDto>>>;
