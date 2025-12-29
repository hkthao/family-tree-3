using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser, IPrivacyService privacyService) : IRequestHandler<GetMemberByIdQuery, Result<MemberDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<MemberDetailDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        // Apply MemberAccessSpecification first
        var baseQuery = _context.Members.AsQueryable()
            .WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));

        var spec = new MemberByIdSpecification(request.Id);
        spec.Query.Include(m => m.SourceRelationships);
        spec.Query.Include(m => m.TargetRelationships);
        spec.Query.Include(m => m.Family); // NEW

        // Apply MemberByIdSpecification to the already filtered baseQuery
        var query = baseQuery.WithSpecification(spec);

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
