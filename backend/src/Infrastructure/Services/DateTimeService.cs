using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}
