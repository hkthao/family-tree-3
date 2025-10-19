using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using FamilyTree.Domain.Entities;

namespace FamilyTree.Application.SystemConfigurations.Commands.CreateSystemConfiguration;

public record CreateSystemConfigurationCommand : IRequest<Result<int>>
{
    public string Key { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string? Description { get; init; }
    public string ValueType { get; init; } = "string";
}

public class CreateSystemConfigurationCommandHandler : IRequestHandler<CreateSystemConfigurationCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateSystemConfigurationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entity = new SystemConfiguration
        {
            Key = request.Key,
            Value = request.Value,
            Description = request.Description,
            ValueType = request.ValueType
        };

        _context.SystemConfigurations.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
