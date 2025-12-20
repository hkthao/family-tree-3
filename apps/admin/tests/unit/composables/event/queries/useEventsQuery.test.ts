import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useEventsQuery } from '@/composables/event/queries/useEventsQuery';
import { useQuery } from '@tanstack/vue-query';
import { ref, type Ref, computed } from 'vue';
import type { Event, EventFilter, Paginated } from '@/types';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import { queryKeys } from '@/constants/queryKeys';
import { EventType, CalendarType, RepeatRule } from '@/types'; // Import necessary enums

// Mock external dependencies
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
}));

// Mock eventService
const mockEventService: EventServiceAdapter = {
  add: vi.fn(),
  update: vi.fn(),
  delete: vi.fn(),
  getById: vi.fn(),
  search: vi.fn(),
  getEventsByFamilyId: vi.fn(),
  getByIds: vi.fn(),
};

describe('useEventsQuery', () => {
  const mockEvent: Event = {
    id: 'event1',
    name: 'Test Event',
    code: 'TE001',
    type: EventType.Other,
    familyId: 'family1',
    calendarType: CalendarType.Solar,
    solarDate: new Date('2023-01-01'),
    lunarDate: null,
    repeatRule: RepeatRule.None,
    description: 'A test description',
    color: '#FF0000',
    relatedMemberIds: ['member1'],
  };

  const mockPaginatedEvents: Paginated<Event> = {
    items: [mockEvent],
    totalItems: 1,
    totalPages: 1,
    page: 1,
    itemsPerPage: 10,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should call eventService.search with mapped list and filter options in queryFn', async () => {
    const filtersRef: Ref<EventFilter> = ref({
      searchQuery: 'test',
      page: 1,
      itemsPerPage: 10,
      sortBy: [{ key: 'name', order: 'asc' }],
      familyId: 'family1',
      type: EventType.Other,
      calendarType: CalendarType.Solar,
      lunarMonthRange: [1, 12],
    });

    // Mock useQuery to immediately execute queryFn
    (useQuery as vi.Mock).mockImplementation((options) => {
      options.queryFn();
      return { data: ref(mockPaginatedEvents), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });
    mockEventService.search.mockResolvedValue({ ok: true, value: mockPaginatedEvents });

    useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(mockEventService.search).toHaveBeenCalledWith(
      { page: 1, itemsPerPage: 10, sortBy: [{ key: 'name', order: 'asc' }] },
      {
        searchQuery: 'test',
        familyId: 'family1',
        type: EventType.Other,
        calendarType: CalendarType.Solar,
        lunarMonthRange: [1, 12],
      },
    );
  });

  it('should return events and totalItems on successful query', async () => {
    const filtersRef: Ref<EventFilter> = ref({ page: 1, itemsPerPage: 10 });
    (useQuery as vi.Mock).mockImplementation((options) => {
      options.queryFn();
      return { data: ref(mockPaginatedEvents), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });
    mockEventService.search.mockResolvedValue({ ok: true, value: mockPaginatedEvents });

    const { events, totalItems } = useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(events.value).toEqual([mockEvent]);
    expect(totalItems.value).toBe(1);
  });

  it('should return loading true while fetching', () => {
    const filtersRef: Ref<EventFilter> = ref({ page: 1, itemsPerPage: 10 });
    (useQuery as vi.Mock).mockImplementation(() => {
      return { data: ref(null), isFetching: ref(true), error: ref(null), refetch: vi.fn() };
    });

    const { loading } = useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(loading.value).toBe(true);
  });

  it('should return error on failed query', async () => {
    const filtersRef: Ref<EventFilter> = ref({ page: 1, itemsPerPage: 10 });
    const mockError = new Error('Failed to fetch events');
    (useQuery as vi.Mock).mockImplementation((options) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError));
      return { data: ref(null), isFetching: ref(false), error: ref(mockError), refetch: vi.fn() };
    });
    mockEventService.search.mockResolvedValue({ ok: false, error: mockError });

    const { error } = useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(error.value).toEqual(mockError);
  });

  it('should handle placeholderData correctly', () => {
    const filtersRef: Ref<EventFilter> = ref({ page: 1, itemsPerPage: 10 });
    const previousData: Paginated<Event> = {
      items: [{ ...mockEvent, id: 'previous1' }],
      totalItems: 1, totalPages: 1, page: 1, itemsPerPage: 10,
    };
    let placeholderDataFn: ((previousData: Paginated<Event> | undefined) => Paginated<Event> | undefined) | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      placeholderDataFn = options.placeholderData;
      return { data: ref(mockPaginatedEvents), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });

    useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(placeholderDataFn?.(previousData)).toEqual(previousData);
    expect(placeholderDataFn?.(undefined)).toBeUndefined();
  });

  it('should have the correct queryKey', () => {
    const filtersRef: Ref<EventFilter> = ref({
      searchQuery: 'test',
      page: 1,
      itemsPerPage: 10,
      sortBy: [{ key: 'name', order: 'asc' }],
      familyId: 'family1',
      type: EventType.Other,
    });
    let queryKeyComputed: Ref<any> | undefined;

    (useQuery as vi.Mock).mockImplementation((options) => {
      queryKeyComputed = options.queryKey;
      return { data: ref(mockPaginatedEvents), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });

    useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(queryKeyComputed?.value).toEqual(queryKeys.events.list(filtersRef.value));
  });
});
