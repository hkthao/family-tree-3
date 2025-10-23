using System.Reflection;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Security;

namespace backend.Application.Common.Behaviours;

/// <summary>
/// Hành vi pipeline để xử lý ủy quyền (authorization) cho các yêu cầu.
/// </summary>
/// <typeparam name="TRequest">Kiểu của yêu cầu.</typeparam>
/// <typeparam name="TResponse">Kiểu của phản hồi.</typeparam>
public class AuthorizationBehaviour<TRequest, TResponse>(
    IUser user) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Thông tin người dùng hiện tại.
    /// </summary>
    private readonly IUser _user = user;

    /// <summary>
    /// Xử lý ủy quyền cho yêu cầu.
    /// </summary>
    /// <param name="request">Yêu cầu hiện tại.</param>
    /// <param name="next">Delegate để chuyển yêu cầu đến handler tiếp theo trong pipeline.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Phản hồi từ handler tiếp theo.</returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu người dùng chưa được xác thực.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu người dùng không có quyền truy cập.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_user.Id == default)
            {
                throw new UnauthorizedAccessException();
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                // Must be a member of at least one role in roles
                var requiredRoles = authorizeAttributesWithRoles.SelectMany(a => a.Roles.Split(',')).Distinct();
                var authorized = requiredRoles.Any(role => _user.Roles?.Contains(role.Trim()) ?? false);

                if (!authorized)
                    throw new ForbiddenAccessException();
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
