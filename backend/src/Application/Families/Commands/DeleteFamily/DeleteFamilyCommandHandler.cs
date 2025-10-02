using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandler : IRequestHandler<DeleteFamilyCommand>
{
    private readonly IFamilyRepository _familyRepository;

    public DeleteFamilyCommandHandler(IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _familyRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.Id);
        }

        await _familyRepository.DeleteAsync(request.Id);
    }
}
