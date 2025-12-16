using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events;
using FluentValidation;

namespace backend.Application.FamilyLocations.Commands;

public record UpdateFamilyLocationCommand : IRequest<Result>, IMapTo<FamilyLocation>
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public LocationType LocationType { get; set; }
    public LocationAccuracy Accuracy { get; set; }
    public LocationSource Source { get; set; }
}

public class UpdateFamilyLocationCommandValidator : AbstractValidator<UpdateFamilyLocationCommand>
{
    public UpdateFamilyLocationCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId is required.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(v => v.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");
    }
}

public class UpdateFamilyLocationCommandHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<UpdateFamilyLocationCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(UpdateFamilyLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FamilyLocations.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound($"FamilyLocation with id {request.Id} not found.");
        }

        _mapper.Map(request, entity);
        entity.AddDomainEvent(new FamilyLocationUpdatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
