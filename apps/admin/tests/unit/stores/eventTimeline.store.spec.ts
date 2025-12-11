import { setActivePinia, createPinia } from 'pinia';
import { useEventTimelineStore } from '@/stores/eventTimeline.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, Event, EventFilter } from '@/types';
import { ok, err, EventType } from '@/types';
import { createServices } from '@/services/service.factory';
// import i18n from '@/plugins/i18n'; // REMOVED

// Mock the IEventService
const mockSearch = vi.fn();

vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    event: {
      search: mockSearch,
      fetch: vi.fn(),
      getById: vi.fn(),
      add: vi.fn(),
      addItems: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getByIds: vi.fn(),
      getUpcomingEvents: vi.fn(),
    },
    ai: {},
    auth: {},
    chat: {},
    face: {},
    family: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
    familyDict: {},
  })),
}));

vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key),
    },
  },
}));

describe('eventTimeline.store', () => {
  let store: ReturnType<typeof useEventTimelineStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useEventTimelineStore();
    store.$reset();
    store.services = createServices('test');
    mockSearch.mockReset();

    mockSearch.mockResolvedValue(ok({
      items: [],
      page: 1,
      totalItems: 0,
      totalPages: 1,
    }));
  });

  const mockEvent: Event = {
    id: 'event-1',
    name: 'Test Event',
    type: EventType.Other,
    familyId: 'family-1',
    startDate: new Date('2024-01-15T10:00:00Z'),
    endDate: new Date('2024-01-15T12:00:00Z'),
    location: 'Test Location',
    description: 'Test Description',
    color: '#FF0000',
    relatedMembers: ['member-1'],
  };

  const mockPaginatedEvents = {
    items: [mockEvent],
    page: 1,
    totalItems: 1,
    totalPages: 1,
  };

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
      expect(mockSearch).toHaveBeenCalledWith(
        { page: store.list.currentPage, itemsPerPage: store.list.itemsPerPage, sortBy: store.list.sortBy }, // options
        store.list.filters, // filters
      );
    });

    it('should handle load items failure', async () => {
      const errorMessage = 'Failed to load events.';
      mockSearch.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBe('event.errors.load');
      expect(store.list.items).toEqual([]);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });
  });

  describe('setListOptions', () => {
    it('should update list options and call _loadItems', async () => {
      const options = {
        page: 2,
        itemsPerPage: 20,
        sortBy: [{ key: 'name', order: 'desc' }],
      };
      mockSearch.mockResolvedValue(ok({ ...mockPaginatedEvents, page: options.page }));

      store.setListOptions(options);

      expect(store.list.currentPage).toBe(options.page);
      expect(store.list.itemsPerPage).toBe(options.itemsPerPage);
      expect(store.list.sortBy).toEqual(options.sortBy);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should not call _loadItems if options are the same', async () => {
      store.list.currentPage = 1;
      store.list.itemsPerPage = 10;
      store.list.sortBy = [];

      const options = {
        page: 1,
        itemsPerPage: 10,
        sortBy: [],
      };

      store.setListOptions(options);

      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('setFilters', () => {
    it('should update filters and call _loadItems', async () => {
      const newFilters: EventFilter = { familyId: 'family-2' };
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents));

      store.setFilters(newFilters);

      expect(store.list.filters.familyId).toBe('family-2');
      expect(store.list.currentPage).toBe(1); // Should reset page
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });
  });
});
