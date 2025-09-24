import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach } from 'vitest';
import type { FamilyEvent } from '@/types/family';
import { useFamilyEventStore } from '@/stores/family-event.store';
import { createServices } from '@/services/service.factory';
import type { Paginated, Result } from '@/types/common';
import { ok, err } from '@/types/common';
import type { ApiError } from '@/utils/api';
import type { IFamilyEventService, EventFilter } from '@/services/family-event/family-event.service.interface';
import { generateMockFamilyEvents, generateMockFamilyEvent } from '@/data/mock/family-event.mock';
import { simulateLatency } from '@/utils/mockUtils';

export class MockFamilyEventServiceForTest implements IFamilyEventService {
  private _events: FamilyEvent[] = generateMockFamilyEvents(20);

  get events(): FamilyEvent[] {
    return [...this._events];
  }

  async fetch(): Promise<Result<FamilyEvent[], ApiError>> {
    return ok([]);
  }

  async getById(id: string): Promise<Result<FamilyEvent | undefined, ApiError>> {
    const event = this.events.find((e) => e.id === id);
    return ok(await simulateLatency(event));
  }

  async add(newEvent: Omit<FamilyEvent, 'id'>): Promise<Result<FamilyEvent, ApiError>> {
    const eventToAdd: FamilyEvent = {
      ...newEvent,
      id: generateMockFamilyEvent(this._events.length + 1).id,
    };
    this._events.push(eventToAdd);
    return ok(await simulateLatency(eventToAdd));
  }

  async update(updatedEvent: FamilyEvent): Promise<Result<FamilyEvent, ApiError>> {
    const index = this._events.findIndex((e) => e.id === updatedEvent.id);
    if (index !== -1) {
      this._events[index] = updatedEvent;
      return ok(await simulateLatency(updatedEvent));
    }
    return err({ message: 'Event not found', statusCode: 404 });
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return ok(undefined);
  }

  async searchItems(
    filters: EventFilter,
    page?: number,
    itemsPerPage?: number
  ): Promise<Result<Paginated<FamilyEvent>, ApiError>> {
    return ok({
      items: [],
      totalItems: 0,
      totalPages: 0,
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