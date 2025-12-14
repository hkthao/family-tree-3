using backend.Application.AI.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IAiGenerateService
{
    Task<Result<T>> GenerateDataAsync<T>(GenerateRequest request, CancellationToken cancellationToken);
}
