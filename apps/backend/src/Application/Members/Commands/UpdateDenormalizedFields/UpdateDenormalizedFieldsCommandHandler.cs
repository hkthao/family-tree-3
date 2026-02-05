using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Family;
using backend.Application.Common.Models;

namespace backend.Application.Members.Commands.UpdateDenormalizedFields;

public class UpdateDenormalizedFieldsCommandHandler(IApplicationDbContext context, IMemberRelationshipService memberRelationshipService) : IRequestHandler<UpdateDenormalizedFieldsCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMemberRelationshipService _memberRelationshipService = memberRelationshipService;

    public async Task<Result> Handle(UpdateDenormalizedFieldsCommand request, CancellationToken cancellationToken)
    {
        var members = await _context.Members
            .Where(m => m.FamilyId == request.FamilyId)
            .Include(m => m.SourceRelationships)
            .Include(m => m.TargetRelationships)
            .ToListAsync(cancellationToken);

        foreach (var member in members)
        {
            await _memberRelationshipService.UpdateDenormalizedRelationshipFields(member, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
