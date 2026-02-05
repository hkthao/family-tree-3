using backend.Application.Common.Interfaces.Core;

namespace backend.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}
