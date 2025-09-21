import { setActivePinia, createPinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useFamilyEventsStore } from '../familyEvents';
import { MockFamilyEventService, type FamilyEvent } from '../../services/familyEvent.service';

describe('useFamilyEventsStore', () => {
  let mockService: MockFamilyEventService;
  let initialEvents: FamilyEvent[];

  beforeEach(() => {
    setActivePinia(createPinia());
    vi.resetAllMocks();

    initialEvents = [
      { id: '1', familyId: 'f1', name: 'Event A', type: 'Birth', startDate: new Date().toISOString(), createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
      { id: '2', familyId: 'f1', name: 'Event B', type: 'Marriage', startDate: new Date().toISOString(), createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
      { id: '3', familyId: 'f2', name: 'Event C', type: 'Death', startDate: new Date().toISOString(), createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
    ];
    mockService = new MockFamilyEventService(initialEvents);
  });

  it('should fetch all family events', async () => {
    const store = useFamilyEventsStore(mockService);
    await store.fetchAll();

    expect(store.familyEvents.length).toBe(initialEvents.length);
    expect(store.total).toBe(initialEvents.length);
    expect(store.loading).toBe(false);
    expect(mockService.fetchFamilyEvents).toHaveBeenCalledTimes(1);
  });

  it('should add a family event', async () => {
    const store = useFamilyEventsStore(mockService);
    const newEventData = {
      familyId: 'f3',
      name: 'New Event',
      type: 'Other',
      startDate: new Date().toISOString(),
    };

    await store.add(newEventData);

    expect(store.familyEvents.length).toBe(initialEvents.length + 1);
    expect(store.total).toBe(initialEvents.length + 1);
    expect(store.familyEvents.some(e => e.name === newEventData.name)).toBe(true);
    expect(store.loading).toBe(false);
    expect(mockService.addFamilyEvent).toHaveBeenCalledTimes(1);
    expect(mockService.addFamilyEvent).toHaveBeenCalledWith(newEventData);
  });

  it('should update a family event', async () => {
    const store = useFamilyEventsStore(mockService);
    const eventToUpdate = initialEvents[0];
    const updatedName = 'Updated Event Name';

    await store.update(eventToUpdate.id, { name: updatedName });

    expect(store.familyEvents.find(e => e.id === eventToUpdate.id)?.name).toBe(updatedName);
    expect(store.loading).toBe(false);
    expect(mockService.updateFamilyEvent).toHaveBeenCalledTimes(1);
    expect(mockService.updateFamilyEvent).toHaveBeenCalledWith(eventToUpdate.id, { name: updatedName });
  });

  it('should delete a family event', async () => {
    const store = useFamilyEventsStore(mockService);
    const eventToDelete = initialEvents[0];

    await store.remove(eventToDelete.id);

    expect(store.familyEvents.length).toBe(initialEvents.length - 1);
    expect(store.total).toBe(initialEvents.length - 1);
    expect(store.familyEvents.some(e => e.id === eventToDelete.id)).toBe(false);
    expect(store.loading).toBe(false);
    expect(mockService.removeFamilyEvent).toHaveBeenCalledTimes(1);
    expect(mockService.removeFamilyEvent).toHaveBeenCalledWith(eventToDelete.id);
  });

  it('should fetch a single family event by id', async () => {
    const store = useFamilyEventsStore(mockService);
    const eventId = initialEvents[0].id;
    const event = await store.fetchOne(eventId);

    expect(event).toEqual(initialEvents[0]);
    expect(store.loading).toBe(false);
    expect(mockService.fetchFamilyEventById).toHaveBeenCalledTimes(1);
    expect(mockService.fetchFamilyEventById).toHaveBeenCalledWith(eventId);
  });

  it('should handle fetchAll error', async () => {
    const store = useFamilyEventsStore(mockService);
    vi.spyOn(mockService, 'fetchFamilyEvents').mockRejectedValueOnce(new Error('Fetch error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    await store.fetchAll();

    expect(store.error).toBe('Fetch error');
    expect(store.loading).toBe(false);
  });

  it('should handle add error', async () => {
    const store = useFamilyEventsStore(mockService);
    vi.spyOn(mockService, 'addFamilyEvent').mockRejectedValueOnce(new Error('Add error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const newEventData = {
      familyId: 'f3',
      name: 'New Event',
      type: 'Other',
      startDate: new Date().toISOString(),
    };

    await expect(store.add(newEventData)).rejects.toThrow('Add error');
    expect(store.error).toBe('Add error');
    expect(store.loading).toBe(false);
  });

  it('should handle update error', async () => {
    const store = useFamilyEventsStore(mockService);
    vi.spyOn(mockService, 'updateFamilyEvent').mockRejectedValueOnce(new Error('Update error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const eventToUpdate = initialEvents[0];
    const updatedName = 'Updated Event Name';

    await expect(store.update(eventToUpdate.id, { name: updatedName })).rejects.toThrow('Update error');
    expect(store.error).toBe('Update error');
    expect(store.loading).toBe(false);
  });

  it('should handle remove error', async () => {
    const store = useFamilyEventsStore(mockService);
    vi.spyOn(mockService, 'removeFamilyEvent').mockRejectedValueOnce(new Error('Remove error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const eventToDelete = initialEvents[0];

    await expect(store.remove(eventToDelete.id)).rejects.toThrow('Remove error');
    expect(store.error).toBe('Remove error');
    expect(store.loading).toBe(false);
  });

  it('should handle fetchOne error', async () => {
    const store = useFamilyEventsStore(mockService);
    vi.spyOn(mockService, 'fetchFamilyEventById').mockRejectedValueOnce(new Error('Fetch One error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const eventId = initialEvents[0].id;

    const result = await store.fetchOne(eventId);

    expect(result).toBeUndefined();
    expect(store.error).toBe('Fetch One error');
    expect(store.loading).toBe(false);
  });
});
