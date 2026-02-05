using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.UserPushTokens.DTOs;
using backend.Application.UserPushTokens.Specifications;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Queries.SearchUserPushTokens;

public class SearchUserPushTokensQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchUserPushTokensQuery, Result<PaginatedList<UserPushTokenDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<UserPushTokenDto>>> Handle(SearchUserPushTokensQuery request, CancellationToken cancellationToken)
    {
        IQueryable<UserPushToken> query = _context.UserPushTokens.AsNoTracking();

        // Apply UserPushTokenFilterSpecification
        query = query.WithSpecification(new UserPushTokenFilterSpecification(request.UserId, request.Platform, request.IsActive));

        // Apply UserPushTokenSearchQuerySpecification if a search query is provided
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.WithSpecification(new UserPushTokenSearchQuerySpecification(request.SearchQuery));
        }

        // Apply UserPushTokenOrderingSpecification
        query = query.WithSpecification(new UserPushTokenOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedUserPushTokenEntities = await PaginatedList<UserPushToken>.CreateAsync(query, request.Page, request.ItemsPerPage);

        var userPushTokenDtos = _mapper.Map<List<UserPushTokenDto>>(paginatedUserPushTokenEntities.Items);

        var paginatedListDto = new PaginatedList<UserPushTokenDto>(
            userPushTokenDtos,
            paginatedUserPushTokenEntities.TotalItems,
            paginatedUserPushTokenEntities.Page,
            paginatedUserPushTokenEntities.TotalPages
        );

        return Result<PaginatedList<UserPushTokenDto>>.Success(paginatedListDto);
    }
}
