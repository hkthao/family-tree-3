using Microsoft.Extensions.Logging;

namespace backend.Application.Common.Behaviours;

/// <summary>
/// Hành vi pipeline để xử lý các ngoại lệ chưa được xử lý trong quá trình xử lý yêu cầu.
/// </summary>
/// <typeparam name="TRequest">Kiểu của yêu cầu.</typeparam>
/// <typeparam name="TResponse">Kiểu của phản hồi.</typeparam>
public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Đối tượng ghi log.
    /// </summary>
    private readonly ILogger<TRequest> _logger = logger;

    /// <summary>
    /// Xử lý yêu cầu và bắt các ngoại lệ chưa được xử lý.
    /// </summary>
    /// <param name="request">Yêu cầu hiện tại.</param>
    /// <param name="next">Delegate để chuyển yêu cầu đến handler tiếp theo trong pipeline.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Phản hồi từ handler tiếp theo.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "backend Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }
}
