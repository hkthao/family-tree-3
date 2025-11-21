using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using FamilyDictLineage = backend.Domain.Enums.FamilyDictLineage;

namespace backend.Application.FamilyDicts.Commands.CreateFamilyDict;

public class CreateFamilyDictCommandHandler : IRequestHandler<CreateFamilyDictCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public CreateFamilyDictCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateFamilyDictCommand request, CancellationToken cancellationToken)
    {
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

        return entity.Id;
    }
}
