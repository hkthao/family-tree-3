import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach } from 'vitest';
import type { FamilyEvent } from '@/types/family-event';
import type { IFamilyEventService } from '@/services/family-event/family-event.service.interface';
import { generateMockFamilyEvents, generateMockFamilyEvent } from '@/data/mock/family-event.mock';
import type { Paginated } from '@/types/pagination';
import { useFamilyEventStore } from '@/stores/family-event.store';
import { simulateLatency } from '@/utils/mockUtils';
import { createServices } from '@/services/service.factory';

// Create a mock service for testing
class MockFamilyEventServiceForTest implements IFamilyEventService {
  private _familyEvents: FamilyEvent[];

  constructor() {
    this._familyEvents = generateMockFamilyEvents(10); // Initialize with 10 events
  }

  get familyEvents(): FamilyEvent[] {
    return [...this._familyEvents];
  }

  async fetch(): Promise<FamilyEvent[]> {
    return simulateLatency(this.familyEvents);
  }

  async getById(id: string): Promise<FamilyEvent | undefined> {
    return simulateLatency(this.familyEvents.find((event) => event.id === id));
  }

  async add(newItem: Omit<FamilyEvent, 'id'>): Promise<FamilyEvent> {
    const familyEventToAdd = { ...newItem, id: `event-${this._familyEvents.length + 1}` };
    this._familyEvents.push(familyEventToAdd);
    return simulateLatency(familyEventToAdd);
  }

  async update(updatedItem: FamilyEvent): Promise<FamilyEvent> {
    const index = this._familyEvents.findIndex((event) => event.id === updatedItem.id);
    if (index !== -1) {
      this._familyEvents[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Family Event not found');
  }

  async delete(id: string): Promise<void> {
    const initialLength = this._familyEvents.length;
    this._familyEvents = this._familyEvents.filter((event) => event.id !== id);
    if (this._familyEvents.length === initialLength) {
      throw new Error('Family Event not found');
    }
    return simulateLatency(undefined);
  }

  async searchFamilyEvents(
    searchQuery: string,
    familyId?: string,
    page: number = 1,
    itemsPerPage: number = 10
  ): Promise<Paginated<FamilyEvent>> {
    let filtered = this._familyEvents;

    if (familyId) {
      filtered = filtered.filter((event) => event.familyId === familyId);
    }

    if (searchQuery) {
      const lowerCaseSearchQuery = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (event) =>
          event.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (event.description && event.description.toLowerCase().includes(lowerCaseSearchQuery))
      );
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return simulateLatency({
      items,
      totalItems,
      totalPages,
    });
  }

  reset(count: number = 10) {
    this._familyEvents = generateMockFamilyEvents(count);
  }
}

describe('Family Event Store', () => {
  let mockFamilyEventService: MockFamilyEventServiceForTest;

  beforeEach(async () => {
    mockFamilyEventService = new MockFamilyEventServiceForTest();
    mockFamilyEventService.reset(); // Reset mock service state
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useFamilyEventStore();
    store.$reset();
    store.services = createServices('test', { familyEvent: mockFamilyEventService });
    await store._loadFamilyEvents(); // Ensure store is populated before tests run
  });

  it('should have correct state after initial load', () => {
    const store = useFamilyEventStore();
    expect(store.familyEvents.length).toBe(10); // 10 items per page
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.searchTerm).toBe('');
    expect(store.familyIdFilter).toBeUndefined();
    expect(store.totalItems).toBe(10); // Based on mock service initial data
    expect(store.currentFamilyEvent).toBe(null);
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(1); // 10 events, 10 per page
  });

  it('_loadFamilyEvents should populate family events array, totalItems, and totalPages', async () => {
    const store = useFamilyEventStore();
    await store._loadFamilyEvents();
    expect(store.familyEvents.length).toBe(10); // 10 items per page
    expect(store.totalItems).toBe(10); // Based on mock service initial data
    expect(store.familyEvents[0].name).toBe(mockFamilyEventService.familyEvents[0].name);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(1); // 10 events, 10 per page
  });

  it('getFamilyEventById should return the correct family event', async () => {
    const store = useFamilyEventStore();
    await store._loadFamilyEvents();
    const event = store.getFamilyEventById(mockFamilyEventService.familyEvents[0].id);
    expect(event).toBeDefined();
    expect(event?.name).toBe(mockFamilyEventService.familyEvents[0].name);
  });

  it('addFamilyEvent should add a new family event and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadFamilyEvents(); // Initial fetch
    const initialTotalItems = store.totalItems;
    const newEventData: Omit<FamilyEvent, 'id'> = {
      name: 'New Event',
      description: 'This is a new event',
      type: 'Birth',
      startDate: new Date(),
      familyId: mockFamilyEventService.familyEvents[0].familyId,
    };
    await store.addFamilyEvent(newEventData);
    expect(store.totalItems).toBe(initialTotalItems + 1);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(2); // 11 events, 10 per page

    // Now, search for the newly added event to confirm its presence
    await store.searchFamilyEvents('New Event');
    expect(store.totalItems).toBe(1);
    expect(store.familyEvents[0].name).toBe('New Event');
  });

  it('updateFamilyEvent should update an existing family event and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadFamilyEvents();
    const eventToUpdate = store.familyEvents[0];
    if (eventToUpdate) {
      const updatedName = 'Updated Event';
      const updatedEvent: FamilyEvent = { ...eventToUpdate, name: updatedName };
      await store.updateFamilyEvent(updatedEvent);
      const foundEvent = store.getFamilyEventById(eventToUpdate.id);
      expect(foundEvent?.name).toBe(updatedName);
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(1); // Should remain 1
    } else {
      expect.fail('No family event to update.');
    }
  });

  it('deleteFamilyEvent should remove a family event and re-load family events', async () => {
    const store = useFamilyEventStore();
    await store._loadFamilyEvents(); // 10 events, 1 page
    const initialTotalItems = store.totalItems;
    const eventToDeleteId = store.familyEvents[0]?.id;
    if (eventToDeleteId) {
      await store.deleteFamilyEvent(eventToDeleteId);
      expect(store.totalItems).toBe(initialTotalItems - 1);
      expect(store.getFamilyEventById(eventToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(1); // 9 events, 10 per page, still 1 page
    } else {
      expect.fail('No family event to delete.');
    }
  });

  it('searchFamilyEvents should filter family events by search term and reset page', async () => {
    const store = useFamilyEventStore();
    await store._loadFamilyEvents(); // 10 events

    const existingEvent = mockFamilyEventService.familyEvents[0];
    const searchName = existingEvent.name.substring(0, 3);

    await store.searchFamilyEvents(searchName);
    const expectedFilteredCount = mockFamilyEventService.familyEvents.filter(e =>
      e.name.toLowerCase().includes(searchName.toLowerCase()) ||
      (e.description && e.description.toLowerCase().includes(searchName.toLowerCase()))
    ).length;

    expect(store.totalItems).toBe(expectedFilteredCount);
    expect(store.familyEvents.length).toBe(Math.min(store.itemsPerPage, expectedFilteredCount));
    expect(store.currentPage).toBe(1);
    expect(store.searchTerm).toBe(searchName);
  });

  it('searchFamilyEvents should filter family events by familyId', async () => {
    const store = useFamilyEventStore();
    await store._loadFamilyEvents();

    const existingEvent = mockFamilyEventService.familyEvents[0];
    const searchFamilyId = existingEvent.familyId || 'non-existent-family';

    await store.searchFamilyEvents('', searchFamilyId);
    const expectedFilteredCount = mockFamilyEventService.familyEvents.filter(e => e.familyId === searchFamilyId).length;

    expect(store.totalItems).toBe(expectedFilteredCount);
    expect(store.familyEvents.length).toBe(Math.min(store.itemsPerPage, expectedFilteredCount));
    expect(store.currentPage).toBe(1);
    expect(store.familyIdFilter).toBe(searchFamilyId);
  });

  it('setCurrentFamilyEvent should set the current family event', () => {
    const store = useFamilyEventStore();
    const mockEvent: FamilyEvent = generateMockFamilyEvent(999);
    store.setCurrentFamilyEvent(mockEvent);
    expect(store.currentFamilyEvent).toEqual(mockEvent);

    store.setCurrentFamilyEvent(null);
    expect(store.currentFamilyEvent).toBeNull();
  });

  it('setPage should update currentPage and re-load family events', async () => {
    const store = useFamilyEventStore();
    mockFamilyEventService.reset(20); // Generate 20 events for pagination test
    await store._loadFamilyEvents(); // 20 events, 10 per page, 2 pages

    await store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(store.familyEvents.length).toBe(10); // Second page of 10 items
    expect(store.familyEvents[0]?.name).toBe(mockFamilyEventService.familyEvents[10].name); // First item of second page

    // Invalid page (too high)
    await store.setPage(3);
    expect(store.currentPage).toBe(2); // Should remain 2

    // Invalid page (too low) - currentPage should not change
    await store.setPage(0);
    expect(store.currentPage).toBe(2); // Should remain 2
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and re-load family events', async () => {
    const store = useFamilyEventStore();
    mockFamilyEventService.reset(20); // Generate 20 events for pagination test
    await store._loadFamilyEvents(); // 20 events, 10 per page, 2 pages

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