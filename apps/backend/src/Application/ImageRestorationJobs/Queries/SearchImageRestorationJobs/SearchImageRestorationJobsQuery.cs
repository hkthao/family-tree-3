using backend.Application.Common.Extensions; // Added
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Common;
using backend.Domain.Enums;

namespace backend.Application.ImageRestorationJobs.Queries.SearchImageRestorationJobs;

public record SearchImageRestorationJobsQuery : PaginatedQuery, IRequest<PaginatedList<ImageRestorationJobDto>> // Modified IRequest return type
{
    public RestorationStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
}

public class SearchImageRestorationJobsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<SearchImageRestorationJobsQuery, PaginatedList<ImageRestorationJobDto>> // Modified IRequestHandler return type
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMapper _mapper = mapper;

    public async Task<PaginatedList<ImageRestorationJobDto>> Handle(SearchImageRestorationJobsQuery request, CancellationToken cancellationToken) // Modified Handle return type
    {
        var userId = _currentUser.UserId.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            // Changed from Result<PaginatedList<...>>.Failure to throw an exception or handle differently
            // Since IRequest now returns PaginatedList directly, we cannot return a Result.
            // For now, I will throw an exception as a placeholder for proper error handling outside of Result.
            // A better solution would be to wrap PaginatedList in Result, but the request was to match SearchMemoryItemsQuery.
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var query = _context.ImageRestorationJobs
            .Where(j => j.UserId == userId);

        if (request.Status.HasValue)
        {
            query = query.Where(j => j.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(j =>
                j.JobId.ToLower().Contains(searchTermLower) ||
                (j.OriginalImageUrl != null && j.OriginalImageUrl.ToLower().Contains(searchTermLower)) ||
                (j.RestoredImageUrl != null && j.RestoredImageUrl.ToLower().Contains(searchTermLower)));
        }

        var paginatedList = await query
            .OrderByDescending(j => j.Created)
            .ProjectTo<ImageRestorationJobDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage); // Modified property names

        return paginatedList; // Modified return type
    }
}
