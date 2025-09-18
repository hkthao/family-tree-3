using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Members.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Member), request.Id);
        }

        _context.Members.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
