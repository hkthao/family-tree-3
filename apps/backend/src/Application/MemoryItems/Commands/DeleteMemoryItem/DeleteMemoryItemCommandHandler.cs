using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.MemoryItems.Commands.DeleteMemoryItem;

public class DeleteMemoryItemCommandHandler : IRequestHandler<DeleteMemoryItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;

    public DeleteMemoryItemCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IDateTime dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(DeleteMemoryItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MemoryItems
            .FirstOrDefaultAsync(mi => mi.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound();
        }

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedDate = _dateTime.Now;
        entity.DeletedBy = _currentUser.UserId.ToString();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
