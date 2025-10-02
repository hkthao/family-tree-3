using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand>
{
    private readonly IMemberRepository _memberRepository;

    public DeleteMemberCommandHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var entity = await _memberRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Member), request.Id);
        }

        await _memberRepository.DeleteAsync(request.Id);
    }
}
