
using System.Security.Claims;
using backend.Application.Common.Interfaces;
using backend.Domain.Constants;
using backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

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
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var claimsTransformation = scope.ServiceProvider.GetRequiredService<IClaimsTransformation>();
            // var mediator = scope.ServiceProvider.GetRequiredService<IMediator>(); // Removed for now
            var externalId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var name = context.User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(externalId))
            {
                _logger.LogWarning("EnsureUserExistsMiddleware: External ID (sub claim) not found for authenticated user.");
                await _next(context);
                return;
            }

            try
            {
                _logger.LogInformation("EnsureUserExistsMiddleware: Processing authenticated user with external ID: {ExternalId}", externalId);

                // Find or Create UserProfile entity (default profile)
                // Assuming User entity is created implicitly or handled elsewhere
                var user = await dbContext.Users
                    .Include(u => u.Profile)
                    .Include(u => u.Preference)
                    .FirstOrDefaultAsync(u => u.AuthProviderId == externalId);

                if (user == null)
                {
                    user = new User(externalId, email ?? "");
                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                                    _logger.LogInformation("EnsureUserExistsMiddleware: Created new User with ID: {UserId}. Profile ID: {ProfileId}. Preference ID: {PreferenceId}", user.Id, user.Profile?.Id, user.Preference?.Id);
                                }
                                else
                                {
                                    // Update existing profile with latest info from claims if needed
                                    user.UpdateProfile(
                                        externalId,
                                        email ?? "",
                                        name ?? "",
                                        context.User.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
                                        context.User.FindFirst(ClaimTypes.Surname)?.Value ?? "",
                                        context.User.FindFirst(ClaimTypes.MobilePhone)?.Value ?? "",
                                        context.User.FindFirst("picture")?.Value ?? ""
                                    );
                                    // No need to explicitly update preference here, as it's created with default values
                                    // and can be updated by the user later.
                    
                                    await dbContext.SaveChangesAsync(CancellationToken.None);
                                    _logger.LogInformation("EnsureUserExistsMiddleware: Updated existing User {UserId}. Profile ID: {ProfileId}. Preference ID: {PreferenceId}", user.Id, user.Profile?.Id, user.Preference?.Id);
                                }
                    
                                // Store UserId and ProfileId in HttpContext.Items
                                context.Items[HttpContextItemKeys.UserId] = user.Id;
                                context.Items[HttpContextItemKeys.ProfileId] = user.Profile?.Id;
                    
                                // Transform claims
                                var principal = await claimsTransformation.TransformAsync(context.User);
                                context.User = principal; // Update User principal in HttpContext
                    
                                _logger.LogInformation("EnsureUserExistsMiddleware: Successfully processed user {UserId} and profile {ProfileId}", user.Id, user.Profile?.Id);
                    
                                // Đánh dấu là đã xử lý để tránh chạy lại trong cùng một request
                                context.Items[HttpContextItemKeys.UserInfoProcessed] = true;            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnsureUserExistsMiddleware for external ID: {ExternalId}. Details: {Error}", externalId, ex.Message);
            }
        }

        // Chuyển tiếp đến middleware tiếp theo trong pipeline
        await _next(context);
    }
}
