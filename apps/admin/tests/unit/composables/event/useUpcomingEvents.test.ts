// tests/unit/composables/event/useUpcomingEvents.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref, computed } from 'vue';
import { useUpcomingEvents } from '@/composables/event/useUpcomingEvents';
import type { Event, EventFilter, Paginated } from '@/types';
import type { UseEventsQueryReturn } from '@/composables/event/queries/useEventsQuery';

describe('useUpcomingEvents', () => {
  const mockPaginatedEvents: Paginated<Event> = {
    items: [{
      id: '1',
      name: 'Upcoming Event 1',
      description: 'Desc 1',
      familyId: 'f1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date('2025-01-01'),
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

  const mockUseEventsQuery = vi.fn<[filters: Ref<EventFilter>], UseEventsQueryReturn>(
    (filtersRef: Ref<EventFilter>) => {
      // Mock the return value of useEventsQuery
      return {
        query: {
          data: ref(mockPaginatedEvents),
          isFetching: ref(false),
          isError: ref(false),
          error: ref(null),
          refetch: vi.fn(),
        } as any,
        events: computed(() => mockPaginatedEvents.items),
        totalItems: computed(() => mockPaginatedEvents.totalItems),
        loading: ref(false),
        refetch: vi.fn(),
      };
    },
  );

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should correctly derive upcomingEventsFilter from baseFilter', () => {
    const baseFilter = ref<EventFilter>({ familyId: 'testFamily' });
    const { upcomingEvents } = useUpcomingEvents(baseFilter, { useEventsQuery: mockUseEventsQuery });

    expect(mockUseEventsQuery).toHaveBeenCalledTimes(1);
    const calledFilters = mockUseEventsQuery.mock.calls[0][0].value;
    expect(calledFilters).toEqual({
      familyId: 'testFamily',
      itemsPerPage: 100,
      sortBy: [{ key: 'solarDate', order: 'asc' }],
    });
  });

  it('should return correct upcomingEvents, isLoading, isError, error, isFetching, and refetch', () => {
    const baseFilter = ref<EventFilter>({ familyId: 'testFamily' });
    const { upcomingEvents, isLoading, isError, error, isFetching, refetch } = useUpcomingEvents(baseFilter, { useEventsQuery: mockUseEventsQuery });

    expect(upcomingEvents.value).toEqual(mockPaginatedEvents.items);
    expect(isLoading.value).toBe(false);
    expect(isError.value).toBe(false);
    expect(error.value).toBe(null);
    expect(isFetching.value).toBe(false);
    expect(refetch).toBe(mockUseEventsQuery.mock.results[0].value.refetch);
  });

  it('should reflect loading state from useEventsQuery', () => {
    mockUseEventsQuery.mockReturnValueOnce({
      query: { isFetching: ref(true), isError: ref(false), error: ref(null) } as any,
      events: ref([]),
      totalItems: ref(0),
      loading: ref(true), // Simulate loading
      refetch: vi.fn(),
    });

    const baseFilter = ref<EventFilter>({ familyId: 'testFamily' });
    const { isLoading, isFetching } = useUpcomingEvents(baseFilter, { useEventsQuery: mockUseEventsQuery });
    expect(isLoading.value).toBe(true);
    expect(isFetching.value).toBe(true);
  });

  it('should reflect error state from useEventsQuery', () => {
    const mockError = new Error('Fetch failed');
    mockUseEventsQuery.mockReturnValueOnce({
      query: { isFetching: ref(false), isError: ref(true), error: ref(mockError) } as any,
      events: ref([]),
      totalItems: ref(0),
      loading: ref(false),
      refetch: vi.fn(),
    });

    const baseFilter = ref<EventFilter>({ familyId: 'testFamily' });
    const { isError, error } = useUpcomingEvents(baseFilter, { useEventsQuery: mockUseEventsQuery });
    expect(isError.value).toBe(true);
    expect(error.value).toBe(mockError);
  });
});
