using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Events;
using AutoMapper; // Explicitly add AutoMapper
using MediatR; // Explicitly add MediatR

namespace backend.Application.FamilyLocations.Commands.UpdateFamilyLocation;

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
