using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Events.EventOccurrences.Jobs;

public interface IEventNotificationJob
{
    Task Run(CancellationToken cancellationToken = default);
}
