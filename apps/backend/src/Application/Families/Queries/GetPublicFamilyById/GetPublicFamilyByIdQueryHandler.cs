using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families.Specifications;
using backend.Domain.Enums;

namespace backend.Application.Families.Queries.GetPublicFamilyById;

/// <summary>
/// Xử lý truy vấn để lấy thông tin chi tiết của một gia đình công khai theo ID.
/// </summary>
public class GetPublicFamilyByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetPublicFamilyByIdQuery, Result<FamilyDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<FamilyDetailDto>> Handle(GetPublicFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new FamilyByIdSpecification(request.Id);

        var family = await _context.Families
            .AsNoTracking()
            .WithSpecification(spec)
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<FamilyDetailDto>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.Id), ErrorSources.NotFound);
        }

        // Kiểm tra xem gia đình có phải là công khai hay không
        if (family.Visibility != FamilyVisibility.Public.ToString())
        {
            return Result<FamilyDetailDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var familyDto = _mapper.Map<FamilyDetailDto>(family);

        return Result<FamilyDetailDto>.Success(familyDto);
    }
}
