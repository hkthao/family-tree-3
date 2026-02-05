using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.MemoryItems.Commands.CreateMemoryItem;

public class CreateMemoryItemCommandHandler : IRequestHandler<CreateMemoryItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser to check for authenticated state

    public CreateMemoryItemCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateMemoryItemCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result<Guid>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        // Check if family exists
        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        // Authorization: Check if user can access the family
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = new MemoryItem(
            request.FamilyId,
            request.Title,
            request.Description,
            request.HappenedAt,
            request.EmotionalTag,
            request.Location
        );

        foreach (var mediaDto in request.MemoryMedia)
        {
            entity.AddMedia(new MemoryMedia(entity.Id, mediaDto.Url));
        }

        // This part needs adjustment based on where MemoryMedia are truly stored/managed.
        // Assuming MemoryMedia are part of MemoryItem aggregate or handled separately.
        // If DeletedMediaIds refers to existing MemoryMedia, authorization should also apply to them.
        var deleteItems = await _context.MemoryMedia
            .Where(mm => request.DeletedMediaIds.Contains(mm.Id) && mm.MemoryItem.FamilyId == request.FamilyId)
            .ToListAsync(cancellationToken);
        if (deleteItems.Count != 0) // Use Any() for collection check
            _context.MemoryMedia.RemoveRange(deleteItems);

        foreach (var personId in request.PersonIds)
        {
            _context.MemoryPersons.Add(new MemoryPerson(entity.Id, personId));
        }

        _context.MemoryItems.Add(entity);

        // Handle LocationLink
        if (request.LocationId.HasValue)
        {
            var locationLink = LocationLink.Create(
                entity.Id.ToString(), // RefId is MemoryItem Id
                RefType.MemoryItem,   // RefType is MemoryItem
                request.Description ?? string.Empty,  // Use request.Description for LocationLink Description
                request.LocationId.Value,
                LocationLinkType.General // Specify LinkType for memory item
            );
            _context.LocationLinks.Add(locationLink);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
