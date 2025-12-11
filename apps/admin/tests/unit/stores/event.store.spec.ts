import { setActivePinia, createPinia } from 'pinia';
import { useEventStore } from '@/stores/event.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, Event, Paginated } from '@/types';
import { EventType } from '@/types';
import { ok, err } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock the IEventService
const mockFetch = vi.fn();
const mockGetById = vi.fn();
const mockAdd = vi.fn();
const mockUpdate = vi.fn();
const mockDelete = vi.fn();
const mockSearch = vi.fn();
const mockGetByIds = vi.fn();
const mockAddItems = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    event: {
      fetch: mockFetch,
      getById: mockGetById,
      add: mockAdd,
      update: mockUpdate,
      delete: mockDelete,
      search: mockSearch,
      getByIds: mockGetByIds,
      addItems: mockAddItems,
    },
    // Add other services as empty objects if they are not directly used by event.store
    ai: {},
    auth: {},
    chat: {},
    dashboard: {},
    family: {},
    face: {},
    faceMember: {},
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('event.store', () => {
  let store: ReturnType<typeof useEventStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useEventStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');
    // Reset mocks before each test
    mockFetch.mockReset();
    mockGetById.mockReset();
    mockAdd.mockReset();
    mockUpdate.mockReset();
    mockDelete.mockReset();
    mockSearch.mockReset();
    mockGetByIds.mockReset();
    mockAddItems.mockReset();
    mockDelete.mockResolvedValue(ok(undefined));
    mockAddItems.mockResolvedValue(ok(['new-id-1']));
    // mockSearch.mockResolvedValue(ok(mockPaginatedEvents));
  });

  const mockEvent: Event = {
    id: 'event-1',
    familyId: 'family-1',
    name: 'Test Event',
    description: 'An event for testing',
    type: EventType.Other,
    startDate: new Date(),
  };

  const mockPaginatedEvents: Paginated<Event> = {
    items: [mockEvent],
    totalItems: 1,
    totalPages: 1,
    page: 1
  };

  // --- Actions Tests ---

  describe('_loadItems', () => {
    it('should load items successfully', async () => {
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.list.items).toEqual([mockEvent]);
      expect(store.list.totalItems).toBe(1);
      expect(store.list.totalPages).toBe(1);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle load items failure', async () => {
      const errorMessage = 'Failed to load events.';
      mockSearch.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.list.items).toEqual([]);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItem', () => {
    it('should add an item successfully', async () => {
      mockAdd.mockResolvedValue(ok(mockEvent));
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents)); // _loadItems is called after successful add
      await store.addItem({ ...mockEvent });

      expect(store.add.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle add item failure', async () => {
      const errorMessage = 'Failed to add event.';
      mockAdd.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.addItem({ ...mockEvent });

      expect(store.add.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('updateItem', () => {
    it('should update an item successfully', async () => {
      mockUpdate.mockResolvedValue(ok(mockEvent));
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents));

      await store.updateItem(mockEvent);

      expect(store.update.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle update item failure', async () => {
      const errorMessage = 'Failed to update event.';
      mockUpdate.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.updateItem(mockEvent);

      expect(store.update.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('deleteItem', () => {
    it('should delete an item successfully', async () => {
      mockDelete.mockResolvedValue(ok(undefined));
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents));

      const result = await store.deleteItem(mockEvent.id!);

      expect(result.ok).toBe(true);
      expect(store._delete.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle delete item failure', async () => {
      const errorMessage = 'Failed to delete event.';
      mockDelete.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.deleteItem(mockEvent.id!);

      expect(result.ok).toBe(false);
      expect(store._delete.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('getById', () => {
    it('should get an item by ID successfully', async () => {
      mockGetById.mockResolvedValue(ok(mockEvent));

      const result = await store.getById(mockEvent.id!);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toEqual(mockEvent);
      expect(result).toEqual(mockEvent);
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });

    it('should handle get by ID failure', async () => {
      const errorMessage = 'Failed to load event.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getById(mockEvent.id!);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });
  });

  describe('getByIds', () => {
    it('should get items by IDs successfully', async () => {
      mockGetByIds.mockResolvedValue(ok([mockEvent]));

      const result = await store.getByIds([mockEvent.id!]);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockEvent]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });

    it('should handle get by IDs failure', async () => {
      const errorMessage = 'Failed to load events.';
      mockGetByIds.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getByIds([mockEvent.id!]);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(result).toEqual([]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItems', () => {
    it('should add multiple items successfully', async () => {
      mockAddItems.mockResolvedValue(ok(['new-id-1', 'new-id-2']));
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents)); // _loadItems is called after successful add

      const newEvents = [
        { familyId: 'f1', name: 'Event 1', type: EventType.Other, startDate: new Date() },
        { familyId: 'f1', name: 'Event 2', type: EventType.Other, startDate: new Date() },
      ];
      const result = await store.addItems(newEvents);

      expect(result.ok).toBe(true);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAddItems).toHaveBeenCalledWith(newEvents);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle add multiple items failure', async () => {
      const errorMessage = 'Failed to add multiple events.';
      mockAddItems.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const newEvents = [
        { familyId: 'f1', name: 'Event 1', type: EventType.Other, startDate: new Date() },
      ];
      const result = await store.addItems(newEvents);

      expect(result.ok).toBe(false);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockAddItems).toHaveBeenCalledWith(newEvents);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });
});