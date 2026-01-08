using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Events.EventOccurrences.Jobs;

public interface IGenerateEventOccurrencesJob
{
    Task GenerateOccurrences(int year, Guid? familyId, CancellationToken cancellationToken);
}
