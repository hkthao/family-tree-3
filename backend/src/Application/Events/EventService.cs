using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events;
using backend.Domain.Entities;

namespace backend.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;

    public EventService(IEventRepository eventRepository, IMapper mapper)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<EventDto>>> GetAllAsync()
    {
        var events = await _eventRepository.GetAllAsync();
        var eventDtos = _mapper.Map<List<EventDto>>(events);
        return Result<List<EventDto>>.Success(eventDtos);
    }

    public async Task<Result<EventDto>> GetByIdAsync(Guid id)
    {
        var anEvent = await _eventRepository.GetByIdAsync(id);
        var eventDto = _mapper.Map<EventDto>(anEvent);
        return Result<EventDto>.Success(eventDto);
    }

    public async Task<Result<Guid>> CreateAsync(Event anEvent)
    {
        var newEvent = await _eventRepository.CreateAsync(anEvent);
        return Result<Guid>.Success(newEvent.Id);
    }

    public async Task<Result<bool>> UpdateAsync(Event anEvent)
    {
        await _eventRepository.UpdateAsync(anEvent);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await _eventRepository.DeleteAsync(id);
        return Result<bool>.Success(true);
    }

    public async Task<Result<PaginatedList<EventDto>>> SearchAsync(EventFilterModel filter)
    {
        var events = await _eventRepository.GetAllAsync();
        if (!string.IsNullOrEmpty(filter.Name))
        {
            events = events.Where(e => e.Name.Contains(filter.Name)).ToList();
        }

        var paginatedList = PaginatedList<Event>.Create(events.AsQueryable(), filter.PageNumber, filter.PageSize);
        var eventDtos = _mapper.Map<List<EventDto>>(paginatedList.Items);

        return Result<PaginatedList<EventDto>>.Success(new PaginatedList<EventDto>(eventDtos, paginatedList.TotalCount, paginatedList.PageNumber, paginatedList.PageSize));
    }
}
