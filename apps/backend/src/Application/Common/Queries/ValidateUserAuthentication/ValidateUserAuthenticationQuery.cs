using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Common.Queries.ValidateUserAuthentication;

/// <summary>
/// Truy vấn để xác thực người dùng hiện tại có được xác thực hay không.
/// </summary>
public record ValidateUserAuthenticationQuery : IRequest<Result>
{
}
