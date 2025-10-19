using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace FamilyTree.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;

public record UpdateSystemConfigurationCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Key { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string? Description { get; init; }
    public string ValueType { get; init; } = "string";
}

public class UpdateSystemConfigurationCommandHandler : IRequestHandler<UpdateSystemConfigurationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateSystemConfigurationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SystemConfigurations
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result.Failure(new string[] { $"SystemConfiguration with Id {request.Id} not found." });
        }

        entity.Key = request.Key;
        entity.Value = request.Value;
        entity.Description = request.Description;
        entity.ValueType = request.ValueType;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
