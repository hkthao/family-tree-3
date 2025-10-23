using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.Inputs; // Corrected using directive

namespace backend.Application.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommand : IRequest<Result<List<Guid>>>
{
    public List<RelationshipInput> Relationships { get; set; } = []; // Corrected type
}
