using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Common.Models;
using backend.Domain.Entities; // Needed for FamilyLocation entity
using backend.Domain.Enums;
using backend.Domain.Events; // Needed for FamilyLocationCreatedEvent
using FluentValidation;

namespace backend.Application.FamilyLocations.Commands;

public record CreateFamilyLocationCommand : IRequest<Result<Guid>>, IMapTo<FamilyLocation>
{
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

public class CreateFamilyLocationCommandValidator : AbstractValidator<CreateFamilyLocationCommand>
{
    public CreateFamilyLocationCommandValidator()
    {
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

public class CreateFamilyLocationCommandHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<CreateFamilyLocationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<Guid>> Handle(CreateFamilyLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<FamilyLocation>(request);
        entity.AddDomainEvent(new FamilyLocationCreatedEvent(entity));

        _context.FamilyLocations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
