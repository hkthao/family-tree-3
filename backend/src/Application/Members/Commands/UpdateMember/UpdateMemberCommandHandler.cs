using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand>
{
    private readonly IMemberRepository _memberRepository;

    public UpdateMemberCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var entity = await _memberRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Member), request.Id);
        }

        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.Nickname = request.Nickname;
        entity.DateOfBirth = request.DateOfBirth;
        entity.DateOfDeath = request.DateOfDeath;
        entity.PlaceOfBirth = request.PlaceOfBirth;
        entity.PlaceOfDeath = request.PlaceOfDeath;
        entity.Gender = request.Gender;
        entity.AvatarUrl = request.AvatarUrl;
        entity.Occupation = request.Occupation;
        entity.Biography = request.Biography;
        entity.FamilyId = request.FamilyId;
        entity.FatherId = request.FatherId;
        entity.MotherId = request.MotherId;
        entity.SpouseId = request.SpouseId;

        await _memberRepository.UpdateAsync(entity);
    }
}
