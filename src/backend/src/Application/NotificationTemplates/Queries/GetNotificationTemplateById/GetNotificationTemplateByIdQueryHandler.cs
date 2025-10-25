using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Models;
using MediatR;
using backend.Domain.Entities;

namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplateById;

public class GetNotificationTemplateByIdQueryHandler : IRequestHandler<GetNotificationTemplateByIdQuery, Result<NotificationTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetNotificationTemplateByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<NotificationTemplateDto>> Handle(GetNotificationTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(nt => nt.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<NotificationTemplateDto>.Failure(new NotFoundException(nameof(NotificationTemplate), request.Id).Message);
        }

        return Result<NotificationTemplateDto>.Success(_mapper.Map<NotificationTemplateDto>(entity));
    }
}
