using backend.Domain.Entities;
using backend.Domain.ValueObjects;

namespace backend.Domain.Interfaces;

public interface IRelationshipGraph
{
    void BuildGraph(IEnumerable<Member> members, IEnumerable<Relationship> relationships);
    RelationshipPath FindShortestPath(Guid startMemberId, Guid endMemberId);
}
