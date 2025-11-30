using backend.Application.Common.Interfaces;
using backend.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai IDomainEventDispatcher để điều phối các sự kiện miền.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    /// <summary>
    /// Khởi tạo một thể hiện mới của lớp DomainEventDispatcher.
    /// </summary>
    /// <param name="mediator">Giao diện IMediator để gửi các sự kiện.</param>
    /// <param name="logger">Giao diện ILogger để ghi log.</param>
    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Điều phối và xóa tất cả các sự kiện miền từ một tập hợp các thực thể.
    /// </summary>
    /// <param name="entities">Tập hợp các thực thể có chứa sự kiện miền.</param>
    public async Task DispatchAndClearEvents(IEnumerable<BaseEntity> entities)
    {
        var domainEvents = entities
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        if (!domainEvents.Any())
        {
            _logger.LogInformation("Không tìm thấy sự kiện miền nào để điều phối.");
            return;
        }

        _logger.LogInformation("Tìm thấy {Count} sự kiện miền để điều phối.", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Điều phối sự kiện miền: {EventType}", domainEvent.GetType().Name);
            await _mediator.Publish(domainEvent);
        }

        // Xóa các sự kiện sau khi chúng đã được điều phối
        entities.ToList().ForEach(e => e.ClearDomainEvents());
        _logger.LogInformation("Đã xóa tất cả các sự kiện miền khỏi các thực thể.");
    }
}
