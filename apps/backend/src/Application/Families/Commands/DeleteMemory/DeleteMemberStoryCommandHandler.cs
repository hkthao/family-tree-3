using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Localization;

namespace backend.Application.MemberStories.Commands.DeleteMemberStory; // Updated

public class DeleteMemberStoryCommandHandler : IRequestHandler<DeleteMemberStoryCommand, Result> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<DeleteMemberStoryCommandHandler> _localizer; // Updated
    private readonly IDateTime _dateTime;

    public DeleteMemberStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<DeleteMemberStoryCommandHandler> localizer, IDateTime dateTime) // Updated
    {
        _context = context;
        _authorizationService = authorizationService;
        _localizer = localizer;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(DeleteMemberStoryCommand request, CancellationToken cancellationToken) // Updated
    {
        var memberStory = await _context.MemberStories.Include(m => m.Member).FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken); // Updated

        if (memberStory == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"MemberStory with ID {request.Id}"), ErrorSources.NotFound); // Updated
        }

        // Authorization check
        if (!_authorizationService.CanManageFamily(memberStory.Member.FamilyId)) // Updated
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        memberStory.IsDeleted = true; // Soft delete // Updated
        memberStory.DeletedDate = _dateTime.Now; // Set deletion timestamp // Updated

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
