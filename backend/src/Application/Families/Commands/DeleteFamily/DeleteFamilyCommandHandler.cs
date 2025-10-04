using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Application.Common.Models; // Added for Result
using Microsoft.EntityFrameworkCore; // Added for DbUpdateException

namespace backend.Application.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandler : IRequestHandler<DeleteFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteFamilyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Families.FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                return Result.Failure($"Family with ID {request.Id} not found.", "NotFound");
            }

            _context.Families.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"Database error occurred while deleting family: {ex.Message}", "Database");
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result.Failure($"An unexpected error occurred while deleting family: {ex.Message}", "Exception");
        }
    }
}
