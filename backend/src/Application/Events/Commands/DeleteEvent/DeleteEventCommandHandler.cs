using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(request.Id);

        if (entity == null)
        {
            return Result<bool>.Failure($"Event with ID {request.Id} not found.");
        }

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
