using backend.Application.AI.Common;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Events;

namespace backend.Application.Members.Commands.GenerateBiography
{
    public class GenerateBiographyCommandHandler : IRequestHandler<GenerateBiographyCommand, Result<BiographyResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        private readonly IAuthorizationService _authorizationService;

        public GenerateBiographyCommandHandler(
            IApplicationDbContext context,
            IUser user,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _user = user;
            _authorizationService = authorizationService;
        }

        public async Task<Result<BiographyResultDto>> Handle(GenerateBiographyCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _user.Id;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Result<BiographyResultDto>.Failure("User is not authenticated.", "Authentication");
            }

            var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
            if (currentUserProfile == null)
            {
                return Result<BiographyResultDto>.Failure("User profile not found.", "NotFound");
            }

            var member = await _context.Members.FindAsync(request.MemberId, cancellationToken);
            if (member == null)
            {
                return Result<BiographyResultDto>.Failure($"Member with ID {request.MemberId} not found.", "NotFound");
            }

            // Authorization check: User must be a manager of the family the member belongs to, or an admin.
            if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(member.FamilyId, currentUserProfile))
            {
                return Result<BiographyResultDto>.Failure("Access denied. Only family managers or admins can generate biographies.", "Forbidden");
            }

            // AI biography generation functionality has been removed.
            return Result<BiographyResultDto>.Failure("AI biography generation is currently not supported.", "NotImplemented");
        }
    }
}
