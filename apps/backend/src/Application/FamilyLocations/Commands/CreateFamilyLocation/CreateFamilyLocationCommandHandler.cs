using backend.Application.Common.Constants; // ADDED for ErrorMessages and ErrorSources
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities; // Needed for FamilyLocation entity
using backend.Domain.Events; // Needed for FamilyLocationCreatedEvent

namespace backend.Application.FamilyLocations.Commands.CreateFamilyLocation;

public class CreateFamilyLocationCommandHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IAuthorizationService authorizationService) : IRequestHandler<CreateFamilyLocationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Guid>> Handle(CreateFamilyLocationCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result<Guid>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        // 2. Check if the FamilyId exists
        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            return Result<Guid>.NotFound(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        // 3. Authorization: Check if user can access the family
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<Guid>.Forbidden(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = _mapper.Map<FamilyLocation>(request);
        entity.AddDomainEvent(new FamilyLocationCreatedEvent(entity));

        _context.FamilyLocations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
