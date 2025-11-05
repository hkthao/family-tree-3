
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Identity.Commands.EnsureUserExists;

/// <summary>
/// Handler cho EnsureUserExistsCommand
/// </summary>
public class EnsureUserExistsCommandHandler : IRequestHandler<EnsureUserExistsCommand, EnsureUserExistsResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<EnsureUserExistsCommandHandler> _logger;

    public EnsureUserExistsCommandHandler(IApplicationDbContext dbContext, ILogger<EnsureUserExistsCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<EnsureUserExistsResult> Handle(EnsureUserExistsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ExternalId))
        { 
            throw new ArgumentException("External ID không được rỗng.", nameof(request.ExternalId));
        }

        var user = await _dbContext.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.AuthProviderId == request.ExternalId, cancellationToken);

        if (user != null)
        {
            _logger.LogInformation("Tìm thấy người dùng có sẵn với ID: {UserId} cho external ID: {ExternalId}", user.Id, request.ExternalId);
            return new EnsureUserExistsResult { UserId = user.Id, ProfileId = user.Profile?.Id };
        }

        _logger.LogInformation("Tạo người dùng mới cho external ID: {ExternalId}", request.ExternalId);
        user = new User(request.ExternalId, request.Email ?? $"{request.ExternalId}@temp.com", request.Name ?? "", request.FirstName, request.LastName, request.Phone, request.Avatar);
        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Đã tạo người dùng mới với ID: {UserId}. Profile ID: {ProfileId}", user.Id, user.Profile?.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "DbUpdateException xảy ra, có thể do race condition. Người dùng với external ID {ExternalId} có thể đã tồn tại. Đang lấy lại thông tin.", request.ExternalId);
            user = await _dbContext.Users
                .Include(u => u.Profile)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthProviderId == request.ExternalId, cancellationToken);

            if (user == null)
            {
                _logger.LogError(ex, "Không thể lấy thông tin người dùng sau khi race condition xảy ra cho external ID: {ExternalId}", request.ExternalId);
                throw;
            }
        }
        
        return new EnsureUserExistsResult { UserId = user.Id, ProfileId = user.Profile?.Id };
    }
}
