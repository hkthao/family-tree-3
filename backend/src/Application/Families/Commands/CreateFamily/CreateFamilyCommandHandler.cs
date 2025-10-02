using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Guid>
{
    private readonly IFamilyRepository _familyRepository;

    public CreateFamilyCommandHandler(IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task<Guid> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        var entity = new Family
        {
            Name = request.Name!,
            Description = request.Description,
            Address = request.Address,
            AvatarUrl = request.AvatarUrl
        };

        await _familyRepository.AddAsync(entity);

        return entity.Id;
    }
}
