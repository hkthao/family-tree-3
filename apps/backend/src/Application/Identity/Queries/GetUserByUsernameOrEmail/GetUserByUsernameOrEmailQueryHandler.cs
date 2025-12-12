using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Identity.Queries.GetUserByUsernameOrEmail;

/// <summary>
/// Xử lý truy vấn để lấy thông tin người dùng dựa trên tên người dùng hoặc email.
/// </summary>
public class GetUserByUsernameOrEmailQueryHandler : IRequestHandler<GetUserByUsernameOrEmailQuery, Result<UserLookupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserByUsernameOrEmailQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<UserLookupDto>> Handle(GetUserByUsernameOrEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
        {
            return Result<UserLookupDto>.Failure("Username or email cannot be empty.", "Validation");
        }

        var user = await _context.Users
            .Include(u => u.Profile) // Bao gồm thông tin Profile
            .AsNoTracking()
            .Where(u => u.Email == request.UsernameOrEmail || u.Profile!.Name == request.UsernameOrEmail)
            .ProjectTo<UserLookupDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return user == null
            ? Result<UserLookupDto>.NotFound("User not found.")
            : Result<UserLookupDto>.Success(user);
    }
}
