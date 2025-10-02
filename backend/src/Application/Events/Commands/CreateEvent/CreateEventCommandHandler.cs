using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMemberRepository _memberRepository;

    public CreateEventCommandHandler(IEventRepository eventRepository, IMemberRepository memberRepository)
    {
        _eventRepository = eventRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = new Event
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            FamilyId = request.FamilyId,
            Type = request.Type,
            Color = request.Color,
        };

        if (request.RelatedMembers.Any())
        {
            var members = (await _memberRepository.GetAllAsync())
                .Where(m => request.RelatedMembers.Contains(m.Id))
                .ToList();
            entity.RelatedMembers = members;
        }

        await _eventRepository.AddAsync(entity);

        return entity.Id;
    }
}
