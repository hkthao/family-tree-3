using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Identity.Queries.GetUserByUsernameOrEmail;

/// <summary>
/// Xử lý truy vấn để lấy thông tin người dùng dựa trên tên người dùng hoặc email.
/// </summary>
public class GetUserByUsernameOrEmailQueryHandler : IRequestHandler<GetUserByUsernameOrEmailQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserByUsernameOrEmailQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserByUsernameOrEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
        {
            return Result<UserDto>.Failure("Username or email cannot be empty.", "Validation");
        }

        var user = await _context.Users
            .Include(u => u.Profile) // Bao gồm thông tin Profile
            .AsNoTracking()
            .Where(u => u.Email == request.UsernameOrEmail)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return user == null
            ? Result<UserDto>.NotFound("User not found.")
            : Result<UserDto>.Success(user);
    }
}
