using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand>
{
    private readonly IFamilyRepository _familyRepository;

    public UpdateFamilyCommandHandler(IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _familyRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Address = request.Address;
        entity.AvatarUrl = request.AvatarUrl;

        await _familyRepository.UpdateAsync(entity);
    }
}
