using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
using backend.Domain.Entities;

namespace backend.Application.FamilyDicts.Commands.CreateFamilyDict;

public class CreateFamilyDictCommandHandler : IRequestHandler<CreateFamilyDictCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public CreateFamilyDictCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper, IAuthorizationService authorizationService)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    public async Task<Result<Guid>> Handle(CreateFamilyDictCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            return Result<Guid>.Forbidden("Chỉ quản trị viên mới được phép tạo FamilyDict.");
        }

        var entity = new FamilyDict
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            Lineage = request.Lineage,
            SpecialRelation = request.SpecialRelation,
            NamesByRegion = _mapper.Map<NamesByRegion>(request.NamesByRegion),
        };

        _context.FamilyDicts.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
