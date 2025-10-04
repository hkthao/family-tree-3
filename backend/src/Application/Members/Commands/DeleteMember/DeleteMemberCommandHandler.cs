using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added for Result

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Members.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                return Result.Failure($"Member with ID {request.Id} not found.", "NotFound");
            }

            _context.Members.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"Database error occurred while deleting member: {ex.Message}", "Database");
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"An unexpected error occurred while deleting member: {ex.Message}", "Exception");
        }
    }
}
