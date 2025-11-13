
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Users.Specifications;
using Microsoft.Extensions.Logging;

namespace backend.Application.Identity.Commands.CreateNovuSubscriber;

/// <summary>
/// Handler cho CreateNovuSubscriberCommand
/// </summary>
public class CreateNovuSubscriberCommandHandler : IRequestHandler<CreateNovuSubscriberCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationProvider _notificationProvider;
    private readonly ILogger<CreateNovuSubscriberCommandHandler> _logger;

    public CreateNovuSubscriberCommandHandler(IApplicationDbContext dbContext, INotificationProvider notificationProvider, ILogger<CreateNovuSubscriberCommandHandler> logger)
    {
        _dbContext = dbContext;
        _notificationProvider = notificationProvider;
        _logger = logger;
    }

    public async Task Handle(CreateNovuSubscriberCommand request, CancellationToken cancellationToken)
    {
        var userSpec = new UserByIdWithProfileSpec(request.UserId);
        var user = await _dbContext.Users
            .WithSpecification(userSpec)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Không tìm thấy người dùng với ID {UserId}.", request.UserId);
            return;
        }

        // Chỉ đồng bộ nếu người dùng chưa có SubscriberId
        if (!string.IsNullOrEmpty(user.SubscriberId))
        {
            _logger.LogInformation("Bỏ qua đồng bộ Novu vì người dùng {UserId} đã có SubscriberId.", user.Id);
            return;
        }

        try
        {
            var subscriberId = user.Id.ToString();
            _logger.LogInformation("Đang đồng bộ Novu subscriber cho User ID: {UserId} với Subscriber ID: {SubscriberId}", user.Id, subscriberId);

            await _notificationProvider.SyncSubscriberAsync(
                subscriberId,
                user.Profile?.FirstName, // Truy cập qua Profile
                user.Profile?.LastName,  // Truy cập qua Profile
                user.Email,
                user.Profile?.Phone      // Truy cập qua Profile
            );

            // Cập nhật SubscriberId cho người dùng và lưu vào DB
            user.SubscriberId = subscriberId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Đồng bộ và cập nhật Novu SubscriberId thành công cho User ID: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nhưng không ném ra ngoại lệ, vì việc tạo subscriber thất bại không nên làm hỏng toàn bộ request.
            _logger.LogError(ex, "Đồng bộ Novu subscriber thất bại cho User ID {UserId}. Lỗi: {ErrorMessage}", request.UserId, ex.Message);
        }
    }
}
