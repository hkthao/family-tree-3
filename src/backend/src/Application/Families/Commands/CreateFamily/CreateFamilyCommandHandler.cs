using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler(IApplicationDbContext context, ICurrentUser user) : IRequestHandler<CreateFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _user = user;

    public async Task<Result<Guid>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Family.Create(
                request.Name,
                request.Code ?? GenerateUniqueCode("FAM"),
                request.Description,
                request.Address,
                request.AvatarUrl,
                request.Visibility,
                _user.UserId
            );

            _context.Families.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entity.Id);
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }

    private string GenerateUniqueCode(string prefix)
    {
        // For simplicity, generate a GUID and take a substring.
        // In a real application, you'd want to ensure uniqueness against existing codes in the database.
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
