using backend.Application.AI.Common;
using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.GenerateBiography
{
    /// <summary>
    /// Command to generate a biography for a member using AI.
    /// </summary>
    public class GenerateBiographyCommand : IRequest<Result<BiographyResultDto>>
    {
        public Guid MemberId { get; set; }
    }
}
