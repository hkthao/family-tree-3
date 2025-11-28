using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Localization;

namespace backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated

public class UpdateMemberStoryCommandHandler : IRequestHandler<UpdateMemberStoryCommand, Result> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<UpdateMemberStoryCommandHandler> _localizer; // Updated

    public UpdateMemberStoryCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, IStringLocalizer<UpdateMemberStoryCommandHandler> localizer) // Updated
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _localizer = localizer;
    }

    public async Task<Result> Handle(UpdateMemberStoryCommand request, CancellationToken cancellationToken) // Updated
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.MemberId)) // Assuming memberId can be used to check family access
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var memberStory = await _context.MemberStories.FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken); // Updated

        if (memberStory == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"MemberStory with ID {request.Id}"), ErrorSources.NotFound); // Updated
        }

        _mapper.Map(request, memberStory); // Map command to existing entity // Updated

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
