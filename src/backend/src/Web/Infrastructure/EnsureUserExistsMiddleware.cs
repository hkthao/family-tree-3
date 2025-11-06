
using System.Security.Claims;
using backend.Application.Identity.Commands.EnsureUserExists;
using backend.Domain.Constants;
using Microsoft.AspNetCore.Authentication;

namespace backend.Web.Infrastructure;

/// <summary>
/// Middleware để đảm bảo thông tin người dùng tồn tại và được đồng bộ sau khi xác thực thành công.
/// Middleware này sẽ chạy sau khi middleware xác thực, đảm bảo `context.User` đã được điền.
/// Nó chịu trách nhiệm cho việc chuyển đổi claims và phát đi sự kiện đăng nhập của người dùng,
/// đồng thời đảm bảo logic này chỉ chạy một lần cho mỗi request.
/// </summary>
public class EnsureUserExistsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnsureUserExistsMiddleware> _logger;

    public EnsureUserExistsMiddleware(RequestDelegate next, ILogger<EnsureUserExistsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        // Chỉ xử lý nếu người dùng đã được xác thực và chưa được xử lý trong request này
        if (context.User.Identity?.IsAuthenticated == true && !context.Items.ContainsKey(HttpContextItemKeys.UserInfoProcessed))
        {
            using var scope = serviceProvider.CreateScope();
            var claimsTransformation = scope.ServiceProvider.GetRequiredService<IClaimsTransformation>();
            var claimsPrincipal = await claimsTransformation.TransformAsync(context.User);
            
            var externalId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(externalId))
            {
                _logger.LogWarning("EnsureUserExistsMiddleware: External ID (sub claim) không tìm thấy cho người dùng đã xác thực.");
                await _next(context);
                return;
            }

            try
            {
                _logger.LogInformation("EnsureUserExistsMiddleware: Đang xử lý người dùng đã xác thực với external ID: {ExternalId}", externalId);

                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var command = new EnsureUserExistsCommand
                {
                    ExternalId = externalId,
                    Email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value,
                    Name = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value,
                    FirstName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value,
                    LastName = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value,
                    Phone = claimsPrincipal.FindFirst(ClaimTypes.MobilePhone)?.Value,
                    Avatar = claimsPrincipal.FindFirst("picture")?.Value
                };

                var result = await mediator.Send(command);

                // Lưu UserId và ProfileId vào HttpContext.Items
                context.Items[HttpContextItemKeys.UserId] = result.UserId;
                context.Items[HttpContextItemKeys.ProfileId] = result.ProfileId;

                // Chuyển đổi claims
                var principal = await claimsTransformation.TransformAsync(context.User);
                context.User = principal; // Cập nhật User principal trong HttpContext

                _logger.LogInformation("EnsureUserExistsMiddleware: Đã xử lý thành công user {UserId} và profile {ProfileId}", result.UserId, result.ProfileId);

                // Đánh dấu là đã xử lý để tránh chạy lại trong cùng một request
                context.Items[HttpContextItemKeys.UserInfoProcessed] = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong EnsureUserExistsMiddleware cho external ID: {ExternalId}. Chi tiết: {Error}", externalId, ex.Message);
            }
        }

        // Chuyển tiếp đến middleware tiếp theo trong pipeline
        await _next(context);
    }
}
