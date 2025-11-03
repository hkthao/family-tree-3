using System.Diagnostics;
using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Behaviours;

/// <summary>
/// Hành vi pipeline để theo dõi hiệu suất của các yêu cầu.
/// </summary>
/// <typeparam name="TRequest">Kiểu của yêu cầu.</typeparam>
/// <typeparam name="TResponse">Kiểu của phản hồi.</typeparam>
public class PerformanceBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, ICurrentUser user) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Đối tượng ghi log.
    /// </summary>
    private readonly ILogger<TRequest> _logger = logger;
    /// <summary>
    /// Thông tin người dùng hiện tại.
    /// </summary>
    private readonly ICurrentUser _user = user;

    /// <summary>
    /// Xử lý yêu cầu và ghi log nếu thời gian xử lý vượt quá ngưỡng.
    /// </summary>
    /// <param name="request">Yêu cầu hiện tại.</param>
    /// <param name="next">Delegate để chuyển yêu cầu đến handler tiếp theo trong pipeline.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Phản hồi từ handler tiếp theo.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        var elapsedMilliseconds = timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _user.UserId;

            _logger.LogWarning("backend Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                requestName, elapsedMilliseconds, userId.ToString(), request);
        }

        return response;
    }
}
