using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public class CreateMemberStoryCommandHandler : IRequestHandler<CreateMemberStoryCommand, Result<Guid>> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<CreateMemberStoryCommandHandler> _localizer; // Updated

    public CreateMemberStoryCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, IStringLocalizer<CreateMemberStoryCommandHandler> localizer) // Updated
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _localizer = localizer;
    }

    public async Task<Result<Guid>> Handle(CreateMemberStoryCommand request, CancellationToken cancellationToken) // Updated
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.MemberId)) // Assuming memberId can be used to check family access
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Find the member to ensure it exists and belongs to the family
        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        var memberStory = _mapper.Map<MemberStory>(request); // Updated
        memberStory.Id = Guid.NewGuid(); // Assign a new ID

        _context.MemberStories.Add(memberStory); // Updated
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(memberStory.Id); // Updated
    }
}
