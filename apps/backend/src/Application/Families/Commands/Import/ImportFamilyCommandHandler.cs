using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Families.Commands.Import;

public class ImportFamilyCommandHandler : IRequestHandler<ImportFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser; // Changed from ICurrentUserService to ICurrentUser

    public ImportFamilyCommandHandler(IApplicationDbContext context, ICurrentUser currentUser) // Changed constructor parameter
    {
        _context = context;
        _currentUser = currentUser; // Assign to _currentUser
    }

    public async Task<Result<Guid>> Handle(ImportFamilyCommand request, CancellationToken cancellationToken)
    {
        if (request.FamilyId.HasValue)
        {
            var existingFamily = await _context.Families.FindAsync(new object[] { request.FamilyId.Value }, cancellationToken);
            if (existingFamily == null)
            {
                return Result<Guid>.Failure($"Family with ID {request.FamilyId.Value} not found.");
            }

            existingFamily.UpdateFamilyDetails(
                request.FamilyData.Name,
                request.FamilyData.Description,
                request.FamilyData.Address,
                request.FamilyData.Visibility, // Use visibility from DTO
                request.FamilyData.Code,
                request.FamilyData.GenealogyRecord,
                request.FamilyData.ProgenitorName,
                request.FamilyData.FamilyCovenant,
                request.FamilyData.ContactInfo
            );

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(existingFamily.Id);
        }
        else
        {
            var newFamily = Family.Create(
                request.FamilyData.Name,
                request.FamilyData.Code,
                request.FamilyData.Description,
                request.FamilyData.Address,
                request.FamilyData.Visibility, // Use visibility from DTO
                _currentUser.UserId,
                "System", // Default source
                false, // Not verified by default
                request.FamilyData.GenealogyRecord,
                request.FamilyData.ProgenitorName,
                request.FamilyData.FamilyCovenant,
                request.FamilyData.ContactInfo
            );

            _context.Families.Add(newFamily);

            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(newFamily.Id);
        }
    }
}
