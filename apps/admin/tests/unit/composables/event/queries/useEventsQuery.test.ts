// tests/unit/composables/event/queries/useEventsQuery.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useEventsQuery } from '@/composables/event/queries/useEventsQuery';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import type { Event, EventFilter, Paginated, ListOptions, FilterOptions } from '@/types';
import { success, failure } from '@/utils/result';

// Mock useQuery
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
}));

describe('useEventsQuery', () => {
  let mockEventService: EventServiceAdapter;
  const mockPaginatedEvents: Paginated<Event> = {
    items: [{
      id: '1',
      name: 'Test Event 1',
      description: 'Desc 1',
      familyId: 'f1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date(),
      repeatRule: 'None',
      relatedMemberIds: [],
      color: '#000000',
    }],
    totalItems: 1,
    totalPages: 1,
    pageNumber: 1,
    pageSize: 10,
    hasPreviousPage: false,
    hasNextPage: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();

    mockEventService = {
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getById: vi.fn(),
      search: vi.fn(),
    };

    // Mock useQuery to control its return values
    vi.mocked(useQuery).mockReturnValue({
      data: ref(mockPaginatedEvents),
      isFetching: ref(false),
      error: ref(null),
      refetch: vi.fn(),
    } as any);
  });

  it('should call eventService.search with correct options and return paginated events', async () => {
    const filtersRef = ref<EventFilter>({ page: 1, itemsPerPage: 10, familyId: 'f1', searchQuery: 'Test' });
    vi.mocked(mockEventService.search).mockResolvedValue(success(mockPaginatedEvents));

    const { events, totalItems, loading } = useEventsQuery(filtersRef, { eventService: mockEventService });

    const queryFn = vi.mocked(useQuery).mock.calls[0][0].queryFn;
    const result = await queryFn();

    const expectedListOptions: ListOptions = { page: 1, itemsPerPage: 10, sortBy: undefined };
    const expectedFilterOptions: FilterOptions = {
      searchQuery: 'Test',
      familyId: 'f1',
      type: undefined,
      memberId: undefined,
      startDate: undefined,
      endDate: undefined,
      calendarType: undefined,
      lunarStartDay: undefined,
      lunarStartMonth: undefined,
      lunarEndDay: undefined,
      lunarEndMonth: undefined,
    };

    expect(mockEventService.search).toHaveBeenCalledWith(expectedListOptions, expectedFilterOptions);
    expect(result).toEqual(mockPaginatedEvents);
    expect(events.value).toEqual(mockPaginatedEvents.items);
    expect(totalItems.value).toBe(mockPaginatedEvents.totalItems);
    expect(loading.value).toBe(false);
  });

  it('should throw an error if eventService.search fails', async () => {
    const errorResult = failure(new Error('API Error'));
    vi.mocked(mockEventService.search).mockResolvedValue(errorResult);
    const queryFn = vi.mocked(useQuery).mock.calls[0][0].queryFn;
    await expect(queryFn()).rejects.toThrow('API Error');
  });

  it('should use placeholderData', () => {
    const placeholderDataFn = vi.mocked(useQuery).mock.calls[0][0].placeholderData;
    const previousData = { ...mockPaginatedEvents, totalItems: 5 };
    expect(placeholderDataFn(previousData)).toEqual(previousData);
    expect(placeholderDataFn(undefined)).toBeUndefined();
  });

  it('should reflect loading state', () => {
    const filtersRef = ref<EventFilter>({ page: 1, itemsPerPage: 10, familyId: 'f1' });
    vi.mocked(useQuery).mockReturnValue({
      data: ref(null),
      isFetching: ref(true), // Simulate loading
      error: ref(null),
      refetch: vi.fn(),
    } as any);

    const { loading } = useEventsQuery(filtersRef, { eventService: mockEventService });
    expect(loading.value).toBe(true);
  });
});
