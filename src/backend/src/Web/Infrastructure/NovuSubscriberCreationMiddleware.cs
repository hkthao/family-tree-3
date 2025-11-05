using backend.Application.Identity.Commands.CreateNovuSubscriber;
using backend.Domain.Constants;
using MediatR;

namespace backend.Web.Infrastructure;

/// <summary>
/// Middleware để đảm bảo người dùng được đăng ký làm subscriber trên Novu.
/// Middleware này chạy sau EnsureUserExistsMiddleware và sử dụng UserId đã được xác định.
/// </summary>
public class NovuSubscriberCreationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NovuSubscriberCreationMiddleware> _logger;

    public NovuSubscriberCreationMiddleware(RequestDelegate next, ILogger<NovuSubscriberCreationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        // Chỉ chạy nếu UserId đã được xác định và chưa được xử lý trong request này
        if (context.Items.TryGetValue(HttpContextItemKeys.UserId, out var userIdObj) && userIdObj is Guid userId && userId != Guid.Empty && !context.Items.ContainsKey(HttpContextItemKeys.NovuSubscriberProcessed))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                _logger.LogInformation("NovuSubscriberCreationMiddleware: Kích hoạt tạo subscriber cho User ID: {UserId}", userId);

                var command = new CreateNovuSubscriberCommand { UserId = userId };
                await mediator.Send(command);

                // Đánh dấu là đã xử lý để tránh chạy lại
                context.Items[HttpContextItemKeys.NovuSubscriberProcessed] = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong NovuSubscriberCreationMiddleware cho user ID: {UserId}. Chi tiết: {Error}", userIdObj, ex.Message);
                // Không re-throw để tránh làm gián đoạn request chính
            }
        }

        await _next(context);
    }
}