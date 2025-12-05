using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.FamilyDicts.Commands.UpdateFamilyDict;

public class UpdateFamilyDictCommandHandler : IRequestHandler<UpdateFamilyDictCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public UpdateFamilyDictCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(UpdateFamilyDictCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            return Result.Forbidden("Chỉ quản trị viên mới được phép cập nhật FamilyDict.");
        }

        var entity = await _context.FamilyDicts
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound($"FamilyDict with ID {request.Id} not found.");
        }

        entity.Name = request.Name;
        entity.Type = request.Type;
        entity.Description = request.Description;
        entity.Lineage = request.Lineage;
        entity.SpecialRelation = request.SpecialRelation;

        // Update NamesByRegion
        if (entity.NamesByRegion == null)
        {
            entity.NamesByRegion = _mapper.Map<NamesByRegion>(request.NamesByRegion);
        }
        else
        {
            _mapper.Map(request.NamesByRegion, entity.NamesByRegion);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
