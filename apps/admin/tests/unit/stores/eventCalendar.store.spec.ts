import { setActivePinia, createPinia } from 'pinia';
import { useEventCalendarStore } from '@/stores/eventCalendar.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, Event, EventFilter } from '@/types';
import { ok, err, EventType } from '@/types';
import { createServices } from '@/services/service.factory';
// import i18n from '@/plugins/i18n'; // Import i18n to mock it - REMOVED

// Mock the IEventService
const mockSearch = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    event: {
      search: mockSearch,
      // Add other event service methods as empty objects if they are not directly used by eventCalendar.store
      fetch: vi.fn(),
      getById: vi.fn(),
      add: vi.fn(),
      addItems: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getByIds: vi.fn(),
      getUpcomingEvents: vi.fn(),
    },
    // Add other services as empty objects if they are not directly used by eventCalendar.store
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

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key), // Mock the translation function to return the key itself
    },
  },
}));

describe('eventCalendar.store', () => {
  let store: ReturnType<typeof useEventCalendarStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useEventCalendarStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');
    // Reset mocks before each test
    mockSearch.mockReset();

    // Default mock resolved values
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
      store.list.currentMonthStartDate = new Date('2024-01-01');
      store.list.currentMonthEndDate = new Date('2024-01-31');
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.list.items).toEqual([mockEvent]);
      expect(mockSearch).toHaveBeenCalledTimes(1);
      expect(mockSearch).toHaveBeenCalledWith(
        { page: 1, itemsPerPage: 100 }, // options
        { ...store.list.filters, startDate: store.list.currentMonthStartDate, endDate: store.list.currentMonthEndDate }, // filters
      );
    });

    it('should handle load items failure', async () => {
      store.list.currentMonthStartDate = new Date('2024-01-01');
      store.list.currentMonthEndDate = new Date('2024-01-31');
      const errorMessage = 'Failed to load events.';
      mockSearch.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBe('event.errors.load');
      expect(store.list.items).toEqual([]);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should not load items if currentMonthStartDate or currentMonthEndDate are null', async () => {
      store.list.currentMonthStartDate = null;
      store.list.currentMonthEndDate = null;

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('setFilters', () => {
    it('should update filters and call _loadItems', () => { // Removed async
      store.list.currentMonthStartDate = new Date('2024-01-01');
      store.list.currentMonthEndDate = new Date('2024-01-31');
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents));
      const newFilters: EventFilter = { familyId: 'family-2' };

      store.setFilters(newFilters); // Removed await

      expect(store.list.filters.familyId).toBe('family-2');
      expect(store.list.currentPage).toBe(1); // Should reset page
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });
  });

  describe('setCurrentMonth', () => {
    it('should set current month start/end dates and call _loadItems', async () => {
      mockSearch.mockResolvedValue(ok(mockPaginatedEvents));
      const testDate = new Date('2024-02-10');
      // Using date-fns startOfMonth and endOfMonth directly to get the local time based dates
      const expectedStartDate = new Date(testDate);
      expectedStartDate.setDate(1);
      expectedStartDate.setHours(0, 0, 0, 0);

      const expectedEndDate = new Date(testDate);
      expectedEndDate.setMonth(expectedEndDate.getMonth() + 1, 0); // Last day of the month
      expectedEndDate.setHours(23, 59, 59, 999);

      await store.setCurrentMonth(testDate);

      expect(store.list.currentMonthStartDate?.getTime()).toBe(expectedStartDate.getTime());
      expect(store.list.currentMonthEndDate?.getTime()).toBe(expectedEndDate.getTime());
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });
  });
});
