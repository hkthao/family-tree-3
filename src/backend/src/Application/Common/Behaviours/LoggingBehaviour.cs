using backend.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Behaviours;

/// <summary>
/// Hành vi pipeline để ghi log thông tin yêu cầu trước khi xử lý.
/// </summary>
/// <typeparam name="TRequest">Kiểu của yêu cầu.</typeparam>
public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, IUser user) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// Đối tượng ghi log.
    /// </summary>
    private readonly ILogger _logger = logger;
    /// <summary>
    /// Thông tin người dùng hiện tại.
    /// </summary>
    private readonly IUser _user = user;

    /// <summary>
    /// Xử lý yêu cầu trước khi nó được gửi đến handler.
    /// </summary>
    /// <param name="request">Yêu cầu hiện tại.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Một Task hoàn thành.</returns>
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id;

        _logger.LogInformation("backend Request: {Name} {@UserId} {@Request}",
            requestName, userId?.ToString(), request);

        return Task.CompletedTask;
    }
}
