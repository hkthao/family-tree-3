using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.MemoryItems.Commands.UpdateMemoryItem;

public class UpdateMemoryItemCommandHandler : IRequestHandler<UpdateMemoryItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser to check for authenticated state

    public UpdateMemoryItemCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateMemoryItemCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        var entity = await _context.MemoryItems
            .FirstOrDefaultAsync(mi => mi.Id == request.Id && mi.FamilyId == request.FamilyId, cancellationToken);
        if (entity == null)
        {
            return Result.NotFound();
        }

        // Authorization: Check if user can access the family
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        entity.Update(
            request.Title,
            request.Description,
            request.HappenedAt,
            request.EmotionalTag,
            request.Location // Pass the new location parameter
        );
        // Handle Media updates
        // Lấy tất cả media hiện có của MemoryItem
        var existingMediaItems = await _context.MemoryMedia
            .Where(mm => mm.MemoryItemId == entity.Id)
            .ToListAsync(cancellationToken);

        // Xóa các media đã được đánh dấu để xóa
        var mediaToDelete = existingMediaItems
            .Where(mm => request.DeletedMediaIds.Contains(mm.Id))
            .ToList();
        if (mediaToDelete.Any())
        {
            _context.MemoryMedia.RemoveRange(mediaToDelete);
        }

        // Cập nhật hoặc thêm media mới
        foreach (var mediaDto in request.MemoryMedia)
        {
            if (mediaDto.Id != Guid.Empty) // Nếu có ID, đây là item đã tồn tại (hoặc được cập nhật)
            {
                var existingMedia = existingMediaItems.FirstOrDefault(mm => mm.Id == mediaDto.Id);
                if (existingMedia != null)
                {
                    // Cập nhật URL nếu có thay đổi
                    if (existingMedia.Url != mediaDto.Url)
                    {
                        existingMedia.Update(mediaDto.Url);
                    }
                }
                else
                {
                    // Nếu item có ID nhưng không tìm thấy trong existingMediaItems, 
                    // có thể là một lỗi hoặc item mới được thêm với ID cụ thể.
                    // Hiện tại, chúng ta sẽ thêm nó như một item mới.
                    _context.MemoryMedia.Add(new MemoryMedia(entity.Id, mediaDto.Url));
                }
            }
            else // Không có ID, đây là item mới
            {
                _context.MemoryMedia.Add(new MemoryMedia(entity.Id, mediaDto.Url));
            }
        }
        // Handle MemoryPersons updates
        var existingMemoryPersons = await _context.MemoryPersons
            .Where(mp => mp.MemoryItemId == entity.Id)
            .ToListAsync(cancellationToken);

        var existingPersonIds = existingMemoryPersons.Select(mp => mp.MemberId).ToHashSet();

        // Remove MemoryPersons no longer in the request
        var personsToRemove = existingMemoryPersons
            .Where(mp => !request.PersonIds.Contains(mp.MemberId))
            .ToList();
        _context.MemoryPersons.RemoveRange(personsToRemove);

        // Add new MemoryPersons
        var personsToAddIds = request.PersonIds
            .Where(personId => !existingPersonIds.Contains(personId))
            .ToList();

        foreach (var personId in personsToAddIds)
        {
            _context.MemoryPersons.Add(new MemoryPerson(entity.Id, personId));
        }

        // Handle LocationLink updates
        var existingLocationLink = await _context.LocationLinks
            .FirstOrDefaultAsync(ll => ll.RefId == entity.Id.ToString() && ll.RefType == RefType.MemoryItem, cancellationToken);

        if (request.LocationId.HasValue)
        {
            if (existingLocationLink != null)
            {
                // Update existing location link
                existingLocationLink.UpdateLocationDetails(
                    request.LocationId.Value,
                    request.Description ?? string.Empty // Use request.Description for LocationLink Description
                );
            }
            else
            {
                // Create new location link
                var newLocationLink = LocationLink.Create(
                    entity.Id.ToString(),
                    RefType.MemoryItem,
                    request.Description ?? string.Empty,
                    request.LocationId.Value,
                    LocationLinkType.General
                );
                _context.LocationLinks.Add(newLocationLink);
            }
        }
        else // No location provided in the request
        {
            if (existingLocationLink != null)
            {
                // Remove existing location link
                _context.LocationLinks.Remove(existingLocationLink);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
