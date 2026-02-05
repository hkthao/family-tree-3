using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Application.FamilyMedias.Queries.Specifications;


namespace backend.Application.FamilyMedias.Queries.GetFamilyMediaById;

public class GetFamilyMediaByIdQueryHandler : IRequestHandler<GetFamilyMediaByIdQuery, Result<FamilyMediaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyMediaByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FamilyMediaDto>> Handle(GetFamilyMediaByIdQuery request, CancellationToken cancellationToken)
    {
        var specification = new FamilyMediaByIdSpecification(request.Id);

        var familyMedia = await _context.FamilyMedia
            .WithSpecification(specification)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (familyMedia == null)
        {
            return Result<FamilyMediaDto>.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.Id}"), ErrorSources.NotFound);
        }

        return Result<FamilyMediaDto>.Success(_mapper.Map<FamilyMediaDto>(familyMedia));
    }
}
