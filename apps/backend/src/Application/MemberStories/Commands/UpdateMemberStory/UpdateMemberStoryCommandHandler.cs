using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.GenerateFamilyKb; // Added
using Microsoft.Extensions.Localization;

namespace backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated

public class UpdateMemberStoryCommandHandler : IRequestHandler<UpdateMemberStoryCommand, Result> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<UpdateMemberStoryCommandHandler> _localizer; // Updated
    private readonly IMediator _mediator; // Added

    public UpdateMemberStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<UpdateMemberStoryCommandHandler> localizer, IMediator mediator) // Updated
    {
        _context = context;
        _authorizationService = authorizationService;
        _localizer = localizer;
        _mediator = mediator; // Added
    }

    public async Task<Result> Handle(UpdateMemberStoryCommand request, CancellationToken cancellationToken) // Updated
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.MemberId)) // Assuming memberId can be used to check family access
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var memberStory = await _context.MemberStories.Include(ms => ms.Member).FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken); // Updated

        if (memberStory == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"MemberStory with ID {request.Id}"), ErrorSources.NotFound); // Updated
        }

        memberStory.Update(
            request.Title,
            request.Story,
            request.Year,
            request.TimeRangeDescription,
            request.IsYearEstimated,
            request.LifeStage,
            request.Location,
            request.StorytellerId,
            request.CertaintyLevel
        );

        await _context.SaveChangesAsync(cancellationToken);

        // Publish notification for story update
        await _mediator.Send(new GenerateFamilyKbCommand(memberStory.Member.FamilyId.ToString(), memberStory.Id.ToString(), KbRecordType.Story), cancellationToken);

        return Result.Success();
    }
}
