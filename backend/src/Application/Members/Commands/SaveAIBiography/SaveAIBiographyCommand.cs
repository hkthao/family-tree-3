using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.Members.Commands.SaveAIBiography
{
    public class SaveAIBiographyCommand : IRequest<Result<Guid>>
    {
        public Guid MemberId { get; set; }
        public BiographyStyle Style { get; set; }
        public string Content { get; init; } = null!;
        public AIProviderType Provider { get; set; }
        public string UserPrompt { get; set; } = null!;
        public bool GeneratedFromDB { get; set; }
        public int TokensUsed { get; set; }
    }
}
