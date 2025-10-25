using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplates;

public class GetNotificationTemplatesQueryHandler : IRequestHandler<GetNotificationTemplatesQuery, Result<PaginatedList<NotificationTemplateDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetNotificationTemplatesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<NotificationTemplateDto>>> Handle(GetNotificationTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.NotificationTemplates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.Where(nt => nt.Subject.Contains(request.SearchQuery) || nt.Body.Contains(request.SearchQuery));
        }

        if (request.EventType.HasValue)
        {
            query = query.Where(nt => nt.EventType == request.EventType.Value);
        }

        if (request.Channel.HasValue)
        {
            query = query.Where(nt => nt.Channel == request.Channel.Value);
        }

        if (request.Format.HasValue)
        {
            query = query.Where(nt => nt.Format == request.Format.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            query = query.Where(nt => nt.LanguageCode == request.LanguageCode);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(nt => nt.IsActive == request.IsActive.Value);
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = request.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(nt => EF.Property<object>(nt, request.SortBy))
                : query.OrderBy(nt => EF.Property<object>(nt, request.SortBy));
        }
        else
        {
            query = query.OrderBy(nt => nt.Created); // Default sort
        }

        var paginatedList = await PaginatedList<NotificationTemplate>.CreateAsync(query, request.PageNumber, request.PageSize);

        var dtos = _mapper.Map<List<NotificationTemplateDto>>(paginatedList.Items);

        return Result<PaginatedList<NotificationTemplateDto>>.Success(new PaginatedList<NotificationTemplateDto>(dtos, paginatedList.TotalItems, paginatedList.Page, paginatedList.TotalPages));
    }
}
