using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommand : IRequest<Result>
{
    public Guid MemberId { get; set; }
    public string BiographyContent { get; set; } = string.Empty;
}
