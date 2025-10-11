using backend.Application.AI.Common;
using backend.Application.Common.Models;

namespace backend.Application.AI.Queries.GetLastAIBiography
{
    public class GetLastAIBiographyQuery : IRequest<Result<AIBiographyDto?>>
    {
        public Guid MemberId { get; init; }
    }
}
