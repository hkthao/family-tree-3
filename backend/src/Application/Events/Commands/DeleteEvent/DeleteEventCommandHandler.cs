using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Event), request.Id);
        }

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
