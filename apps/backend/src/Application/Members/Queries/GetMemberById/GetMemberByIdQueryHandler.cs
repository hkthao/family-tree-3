using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<GetMemberByIdQuery, Result<MemberDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<MemberDetailDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new MemberByIdSpecification(request.Id);
        spec.Query.Include(m => m.SourceRelationships);
        spec.Query.Include(m => m.TargetRelationships);

        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var query = _context.Members.AsQueryable().WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var memberDetailDto = await query
            .ProjectTo<MemberDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberDetailDto == null)
        {
            return Result<MemberDetailDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Id}"), ErrorSources.NotFound);
        }

        // Apply privacy filter
        var filteredMemberDetailDto = await _privacyService.ApplyPrivacyFilter(memberDetailDto, memberDetailDto.FamilyId, cancellationToken);

        return Result<MemberDetailDto>.Success(filteredMemberDetailDto);
    }
}
