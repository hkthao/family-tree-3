using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events;
using FluentValidation;

namespace backend.Application.FamilyLocations.Commands;

public record DeleteFamilyLocationCommand(Guid Id) : IRequest<Result>;

public class DeleteFamilyLocationCommandValidator : AbstractValidator<DeleteFamilyLocationCommand>
{
    public DeleteFamilyLocationCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}

public class DeleteFamilyLocationCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteFamilyLocationCommand, Result>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result> Handle(DeleteFamilyLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FamilyLocations.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound($"FamilyLocation with id {request.Id} not found.");
        }

        _context.FamilyLocations.Remove(entity);
        entity.AddDomainEvent(new FamilyLocationDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
