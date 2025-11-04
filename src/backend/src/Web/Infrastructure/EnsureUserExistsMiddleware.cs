
using System.Security.Claims;
using backend.Application.Common.Interfaces;
using backend.Domain.Constants;
using backend.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

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
            var claimsPrincipal = await claimsTransformation.TransformAsync(context.User);
            var externalId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            var name = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            var firstName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value;
            var phone = claimsPrincipal.FindFirst(ClaimTypes.MobilePhone)?.Value;
            var avatar = claimsPrincipal.FindFirst("picture")?.Value;

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
                    user = new User(externalId, email ?? $"{externalId}@temp.com", name ?? "", firstName, lastName, phone, avatar);
                    dbContext.Users.Add(user);
                    try
                    {
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                        _logger.LogInformation("EnsureUserExistsMiddleware: Created new User with ID: {UserId}. Profile ID: {ProfileId}. Preference ID: {PreferenceId}", user.Id, user.Profile?.Id, user.Preference?.Id);
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                    {
                        // Race condition: user was created by another request. Fetch the existing user.
                        _logger.LogWarning("EnsureUserExistsMiddleware: Race condition detected. User with external ID {ExternalId} already exists. Fetching existing user.", externalId);
                        user = await dbContext.Users
                            .Include(u => u.Profile)
                            .Include(u => u.Preference)
                            .FirstOrDefaultAsync(u => u.AuthProviderId == externalId);

                        if (user == null)
                        {
                            // This should ideally not happen if the previous check failed due to a race condition.
                            // Re-throw if we still can't find the user.
                            throw;
                        }
                    }
                }

                // Store UserId and ProfileId in HttpContext.Items
                context.Items[HttpContextItemKeys.UserId] = user.Id;
                context.Items[HttpContextItemKeys.ProfileId] = user.Profile?.Id;

                // Transform claims
                var principal = await claimsTransformation.TransformAsync(context.User);
                context.User = principal; // Update User principal in HttpContext

                _logger.LogInformation("EnsureUserExistsMiddleware: Successfully processed user {UserId} and profile {ProfileId}", user.Id, user.Profile?.Id);

                // Đánh dấu là đã xử lý để tránh chạy lại trong cùng một request
                context.Items[HttpContextItemKeys.UserInfoProcessed] = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnsureUserExistsMiddleware for external ID: {ExternalId}. Details: {Error}", externalId, ex.Message);
            }
        }

        // Chuyển tiếp đến middleware tiếp theo trong pipeline
        await _next(context);
    }
}
