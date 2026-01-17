using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Families.Queries;

public class GetFamilyByCodeQueryHandler(IApplicationDbContext context) : IRequestHandler<GetFamilyByCodeQuery, Result<FamilyDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<FamilyDto>> Handle(GetFamilyByCodeQuery request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Where(f => f.Code == request.Code)
            .Select(f => new FamilyDto
            {
                Id = f.Id,
                Name = f.Name,
                Code = f.Code,
                Description = f.Description,
                Address = f.Address,
                GenealogyRecord = f.GenealogyRecord,
                ProgenitorName = f.ProgenitorName,
                FamilyCovenant = f.FamilyCovenant,
                ContactInfo = f.ContactInfo,
                AvatarUrl = f.AvatarUrl,
                Visibility = f.Visibility,
                TotalMembers = f.TotalMembers,
                TotalGenerations = f.TotalGenerations,
                Source = f.Source,
                IsVerified = f.IsVerified,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<FamilyDto>.Failure($"Family with code '{request.Code}' not found.");
        }

        return Result<FamilyDto>.Success(family);
    }
}
