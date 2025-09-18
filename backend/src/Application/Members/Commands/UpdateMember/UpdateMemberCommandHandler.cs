using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        Member? entity = await _context.Members.FindAsync([request.Id?? string.Empty], cancellationToken);
        if (entity == null)
            throw new Common.Exceptions.NotFoundException(nameof(Member), request.Id!);
        entity.FullName = request.FullName?? string.Empty;
        entity.DateOfBirth = request.DateOfBirth;
        entity.DateOfDeath = request.DateOfDeath;
        entity.Gender = request.Gender;
        entity.AvatarUrl = request.AvatarUrl;
        entity.PlaceOfBirth = request.PlaceOfBirth;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Generation = request.Generation;
        entity.Biography = request.Biography;
        entity.Metadata = request.Metadata;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
