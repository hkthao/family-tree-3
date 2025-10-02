using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMemberRepository _memberRepository;

    public UpdateEventCommandHandler(IEventRepository eventRepository, IMemberRepository memberRepository)
    {
        _eventRepository = eventRepository;
        _memberRepository = memberRepository;
    }

    public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _eventRepository.GetByIdAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Event), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Location = request.Location;
        entity.FamilyId = request.FamilyId;
        entity.Type = request.Type;
        entity.Color = request.Color;

        if (request.RelatedMembers.Any())
        {
            var members = (await _memberRepository.GetAllAsync())
                .Where(m => request.RelatedMembers.Contains(m.Id))
                .ToList();
            entity.RelatedMembers = members;
        }
        else
        {
            entity.RelatedMembers.Clear();
        }

        await _eventRepository.UpdateAsync(entity);
    }
}
