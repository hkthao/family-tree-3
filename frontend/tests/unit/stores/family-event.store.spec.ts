import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach } from 'vitest';
import type { FamilyEvent } from '@/types/family-event';
import { useFamilyEventStore } from '@/stores/family-event.store';
import { createServices } from '@/services/service.factory';
import type { Paginated } from '@/types/pagination';
import type { IFamilyEventService, EventFilter } from '@/services/family-event/family-event.service.interface';
import { generateMockFamilyEvents, generateMockFamilyEvent } from '@/data/mock/family-event.mock';
import { simulateLatency } from '@/utils/mockUtils';

export class MockFamilyEventServiceForTest implements IFamilyEventService {
  private _events: FamilyEvent[] = generateMockFamilyEvents(20);

  get events(): FamilyEvent[] {
    return [...this._events];
  }

  async fetch(): Promise<FamilyEvent[]> {
    return simulateLatency(this.events);
  }

  async getById(id: string): Promise<FamilyEvent | undefined> {
    const event = this.events.find((e) => e.id === id);
    return simulateLatency(event);
  }

  async add(newEvent: Omit<FamilyEvent, 'id'>): Promise<FamilyEvent> {
    const eventToAdd: FamilyEvent = {
      ...newEvent,
      id: generateMockFamilyEvent(this._events.length + 1).id,
    };
    this._events.push(eventToAdd);
    return simulateLatency(eventToAdd);
  }

  async update(updatedEvent: FamilyEvent): Promise<FamilyEvent> {
    const index = this._events.findIndex((e) => e.id === updatedEvent.id);
    if (index !== -1) {
      this._events[index] = updatedEvent;
      return simulateLatency(updatedEvent);
    }
    throw new Error('Event not found');
  }

  async delete(id: string): Promise<void> {
    const initialLength = this._events.length;
    this._events = this._events.filter((e) => e.id !== id);
    if (this.events.length === initialLength) {
      throw new Error('Event not found');
    }
    return simulateLatency(undefined);
  }

  async searchItems(
    filters: EventFilter,
    page: number = 1,
    itemsPerPage: number = 10
  ): Promise<Paginated<FamilyEvent>> {
    let filteredEvents = this._events;

    if (filters.familyId) {
      filteredEvents = filteredEvents.filter(e => e.familyId === filters.familyId);
    }

    if (filters.searchQuery) {
      const lowerCaseQuery = filters.searchQuery.toLowerCase();
      filteredEvents = filteredEvents.filter(e =>
        e.name.toLowerCase().includes(lowerCaseQuery) ||
        e.description?.toLowerCase().includes(lowerCaseQuery)
      );
    }

    if (filters.type) {
      filteredEvents = filteredEvents.filter(e => e.type === filters.type);
    }

    if (filters.startDate) {
      filteredEvents = filteredEvents.filter(e => e.startDate && new Date(e.startDate) >= new Date(filters.startDate!));
    }

    if (filters.endDate) {
      filteredEvents = filteredEvents.filter(e => e.endDate && new Date(e.endDate) <= new Date(filters.endDate!));
    }

    if (filters.location) {
      const lowerCaseLocation = filters.location.toLowerCase();
      filteredEvents = filteredEvents.filter(e => e.location?.toLowerCase().includes(lowerCaseLocation));
    }

    const totalItems = filteredEvents.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filteredEvents.slice(start, end);

    return simulateLatency({
      items,
      totalItems,
      totalPages,
    });
  }
}

describe('Family Event Store', () => {
  let mockFamilyEventService: MockFamilyEventServiceForTest;

  beforeEach(async () => {
    mockFamilyEventService = new MockFamilyEventServiceForTest();
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useFamilyEventStore();
    store.$reset();
    store.services = createServices('test', { familyEvent: mockFamilyEventService });
    await store._loadItems(); // Ensure store is populated before tests run
  });

  it('should have correct state after initial load', () => {
    const store = useFamilyEventStore();
    expect(store.items.length).toBe(10); // 10 items per page
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.filter).toEqual({});
    expect(store.totalItems).toBe(20); // Based on mock service initial data
    expect(store.currentItem).toBe(null);
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(2); // 20 events, 10 per page
  });

  it('_loadItems should populate family events array, totalItems, and totalPages', async () => {
    const store = useFamilyEventStore();
    await store._loadItems();
    expect(store.items.length).toBe(10); // 10 items per page
    expect(store.totalItems).toBe(20); // Based on mock service initial data
    expect(store.items[0].name).toBe(mockFamilyEventService.events[0].name);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(2); // 20 events, 10 per page
  });

  it('getItemById should return the correct family event', async () => {
    const store = useFamilyEventStore();
    await store._loadItems();
    const event = store.getItemById(mockFamilyEventService.events[0].id);
    expect(event).toBeDefined();
    expect(event?.name).toBe(mockFamilyEventService.events[0].name);
  });

  it('addFamilyEvent should add a new family event and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadItems(); // Initial fetch
    const initialTotalItems = store.totalItems;
    const newEventData: Omit<FamilyEvent, 'id'> = {
      name: 'New Event',
      description: 'This is a new event',
      type: 'Birth',
      startDate: new Date(),
      familyId: mockFamilyEventService.events[0].familyId,
    };
    await store.addItem(newEventData);
    expect(store.totalItems).toBe(initialTotalItems + 1);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(3); // 21 events, 10 per page

    // Now, search for the newly added event to confirm its presence
    await store.searchItems({ searchQuery: 'New Event' });
    expect(store.totalItems).toBe(1);
    expect(store.items[0].name).toBe('New Event');
  });

  it('updateFamilyEvent should update an existing family event and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadItems();
    const eventToUpdate = store.items[0];
    if (eventToUpdate) {
      const updatedName = 'Updated Event';
      const updatedEvent: FamilyEvent = { ...eventToUpdate, name: updatedName };
      await store.updateItem(updatedEvent);
      const foundEvent = store.getItemById(eventToUpdate.id);
      expect(foundEvent?.name).toBe(updatedName);
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(2); // Should remain 2
    } else {
      expect.fail('No family event to update.');
    }
  });

  it('deleteFamilyEvent should remove a family event and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadItems(); // 20 events, 2 pages
    const initialTotalItems = store.totalItems;
    const eventToDeleteId = store.items[0]?.id;
    if (eventToDeleteId) {
      await store.deleteItem(eventToDeleteId);
      expect(store.totalItems).toBe(initialTotalItems - 1);
      expect(store.getItemById(eventToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(2); // 19 events, 10 per page, still 2 pages
    } else {
      expect.fail('No family event to delete.');
    }
  });

  it('searchItems should filter family events by search term and reset page', async () => {
    const store = useFamilyEventStore();
    await store._loadItems(); // 20 events

    const existingEvent = mockFamilyEventService.events[0];
    const searchName = existingEvent.name.substring(0, 3);

    await store.searchItems({ searchQuery: searchName });
    const expectedFilteredCount = mockFamilyEventService.events.filter(e =>
      e.name.toLowerCase().includes(searchName.toLowerCase()) ||
      (e.description && e.description.toLowerCase().includes(searchName.toLowerCase()))
    ).length;

    expect(store.totalItems).toBe(expectedFilteredCount);
    expect(store.items.length).toBe(Math.min(store.itemsPerPage, expectedFilteredCount));
    expect(store.currentPage).toBe(1);
    expect(store.filter.searchQuery).toBe(searchName);
  });

  it('searchItems should filter family events by familyId', async () => {
    const store = useFamilyEventStore();
    await store._loadItems();

    const existingEvent = mockFamilyEventService.events[0];
    const searchFamilyId = existingEvent.familyId || 'non-existent-family';

    await store.searchItems({ familyId: searchFamilyId });
    const expectedFilteredCount = mockFamilyEventService.events.filter(e => e.familyId === searchFamilyId).length;

    expect(store.totalItems).toBe(expectedFilteredCount);
    expect(store.items.length).toBe(Math.min(store.itemsPerPage, expectedFilteredCount));
    expect(store.currentPage).toBe(1);
    expect(store.filter.familyId).toBe(searchFamilyId);
  });

  it('setCurrentFamilyEvent should set the current family event', () => {
    const store = useFamilyEventStore();
    const mockEvent: FamilyEvent = mockFamilyEventService.events[0];
    store.setCurrentItem(mockEvent);
    expect(store.currentItem).toEqual(mockEvent);

    store.setCurrentItem(null);
    expect(store.currentItem).toBeNull();
  });

  it('setPage should update currentPage and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadItems(); // 20 events, 10 per page, 2 pages

    await store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(store.items.length).toBe(10); // Second page of 10 items
    expect(store.items[0]?.name).toBe(mockFamilyEventService.events[10].name); // First item of second page

    // Invalid page (too high)
    await store.setPage(3);
    expect(store.currentPage).toBe(2); // Should remain 2

    // Invalid page (too low) - currentPage should not change
    await store.setPage(0);
    expect(store.currentPage).toBe(2); // Should remain 2
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadItems(); // 20 events, 10 per page, 2 pages

    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(2);

    // Change to 5 items per page
    await store.setItemsPerPage(5); // 20 events, 5 per page -> 4 pages
    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(4);
    expect(store.currentPage).toBe(1);

    // Change to 20 items per page (more than total), current page is 1
    await store.setItemsPerPage(20); // 20 events, 20 per page -> 1 page
    expect(store.itemsPerPage).toBe(20);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1);
  });
});