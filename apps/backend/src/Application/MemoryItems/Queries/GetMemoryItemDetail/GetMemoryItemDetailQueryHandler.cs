using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.MemoryItems.Queries.GetMemoryItemDetail;

public class GetMemoryItemDetailQueryHandler : IRequestHandler<GetMemoryItemDetailQuery, Result<MemoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly IAuthorizationService _authorizationService;

    public GetMemoryItemDetailQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IAuthorizationService authorizationService)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _authorizationService = authorizationService;
    }

    public async Task<Result<MemoryItemDto>> Handle(GetMemoryItemDetailQuery request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result<MemoryItemDto>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        // 2. Initial Fetch (simple): Fetch the MemoryItem by Id and !IsDeleted without any Include statements.
        var memoryItem = await _context.MemoryItems
            .Where(mi => mi.Id == request.Id && !mi.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        // 3. Check for Null: If memoryItem is null, return NotFound.
        if (memoryItem == null)
        {
            return Result<MemoryItemDto>.NotFound();
        }

        // 4. Fetch Related Family (separate query): If memoryItem is found, fetch its Family entity,
        //    including its FamilyUsers, in a *separate* query.
        var family = await _context.Families
            .Include(f => f.FamilyUsers)
            .Where(f => f.Id == memoryItem.FamilyId)
            .FirstOrDefaultAsync(cancellationToken);

        // This should ideally not be null if memoryItem.FamilyId is valid, but good to check.
        if (family == null)
        {
            return Result<MemoryItemDto>.NotFound(); // Or a more specific error like "FamilyNotFound"
        }

        // 5. Access Control Check (in-memory): Perform the access control logic
        var currentUserId = _currentUser.UserId;
        var isAdmin = _authorizationService.IsAdmin();

        var hasAccess = isAdmin || // Admin always has access
                        family.CreatedBy == currentUserId.ToString() || // Family creator has access
                        family.FamilyUsers.Any(fu => fu.UserId == currentUserId && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer)); // User is manager or viewer

        if (!hasAccess)
        {
            return Result<MemoryItemDto>.Forbidden(); // Or NotFound to not leak existence
        }

        // 6. Mapping: If access is granted, map the memoryItem to MemoryItemDto and return Success.
        //    We also need to include MemoryMedia and MemoryPersons for the DTO mapping.
        //    Since we are doing separate queries, we should reload the memoryItem with its children
        //    or ensure they are loaded for the mapper.
        var fullMemoryItem = await _context.MemoryItems
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
                .ThenInclude(mp => mp.Member) // Include Member details for MemoryPersonDto
            .Where(mi => mi.Id == request.Id) // We already confirmed access and existence
            .FirstOrDefaultAsync(cancellationToken);

        if (fullMemoryItem == null)
        {
            // This should ideally not happen after the initial check, but as a safeguard.
            return Result<MemoryItemDto>.NotFound();
        }

        var dto = _mapper.Map<MemoryItemDto>(fullMemoryItem);
        return Result<MemoryItemDto>.Success(dto);
    }
}
