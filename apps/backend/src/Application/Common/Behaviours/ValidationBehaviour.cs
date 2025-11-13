using ValidationException = backend.Application.Common.Exceptions.ValidationException;

namespace backend.Application.Common.Behaviours;

/// <summary>
/// Hành vi pipeline để thực hiện xác thực (validation) cho các yêu cầu.
/// </summary>
/// <typeparam name="TRequest">Kiểu của yêu cầu.</typeparam>
/// <typeparam name="TResponse">Kiểu của phản hồi.</typeparam>
public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Danh sách các trình xác thực cho yêu cầu.
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <summary>
    /// Xử lý xác thực cho yêu cầu.
    /// </summary>
    /// <param name="request">Yêu cầu hiện tại.</param>
    /// <param name="next">Delegate để chuyển yêu cầu đến handler tiếp theo trong pipeline.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Phản hồi từ handler tiếp theo.</returns>
    /// <exception cref="ValidationException">Ném ra nếu có lỗi xác thực.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var validationResults = await Task.WhenAll(
                _validators.Select(v =>
                    v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Count != 0)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
