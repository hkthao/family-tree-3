import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useEventsQuery } from '@/composables/event/queries/useEventsQuery';
import { useQuery } from '@tanstack/vue-query';
import { ref, type Ref } from 'vue';
import type { EventDto, EventFilter, Paginated } from '@/types';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import { queryKeys } from '@/constants/queryKeys';
import { EventType, CalendarType, RepeatRule } from '@/types'; // Import necessary enums

// Mock external dependencies
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
}));

// Mock eventService
const mockEventService: EventServiceAdapter = {
  add: vi.fn() as Mock,
  update: vi.fn() as Mock,
  delete: vi.fn() as Mock,
  getById: vi.fn() as Mock,
  search: vi.fn() as Mock,
  getEventsByFamilyId: vi.fn() as Mock,
  getByIds: vi.fn() as Mock,
};

describe('useEventsQuery', () => {
  const mockEvent: EventDto = {
    id: 'event1',
    name: 'Test EventDto',
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

  const mockPaginatedEvents: Paginated<EventDto> = {
    items: [mockEvent],
    totalItems: 1,
    totalPages: 1,
    page: 1,
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
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn();
      return { data: ref(mockPaginatedEvents), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });
    (mockEventService.search as Mock).mockResolvedValue({ ok: true, value: mockPaginatedEvents });

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
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn();
      return { data: ref(mockPaginatedEvents), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });
    (mockEventService.search as Mock).mockResolvedValue({ ok: true, value: mockPaginatedEvents });

    const { events, totalItems } = useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(events.value).toEqual([mockEvent]);
    expect(totalItems.value).toBe(1);
  });

  it('should return loading true while fetching', () => {
    const filtersRef: Ref<EventFilter> = ref({ page: 1, itemsPerPage: 10 });
    (useQuery as Mock).mockImplementation(() => {
      return { data: ref(null), isFetching: ref(true), error: ref(null), refetch: vi.fn() };
    });

    const { loading } = useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(loading.value).toBe(true);
  });

  it('should return error on failed query', async () => {
    const filtersRef: Ref<EventFilter> = ref({ page: 1, itemsPerPage: 10 });
    const mockError = new Error('Failed to fetch events');
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError));
      return { data: ref(null), isFetching: ref(false), error: ref(mockError), refetch: vi.fn() };
    });
    (mockEventService.search as Mock).mockResolvedValue({ ok: false, error: mockError });

    const { error } = useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(error.value).toEqual(mockError);
  });

  it('should handle placeholderData correctly', () => {
    const filtersRef: Ref<EventFilter> = ref({ page: 1, itemsPerPage: 10 });
    const previousData: Paginated<EventDto> = {
      items: [{ ...mockEvent, id: 'previous1' }],
      totalItems: 1, totalPages: 1, page: 1
    };
    let placeholderDataFn: ((previousData: Paginated<EventDto> | undefined) => Paginated<EventDto> | undefined) | undefined;

    (useQuery as Mock).mockImplementation((options: any) => {
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

    (useQuery as Mock).mockImplementation((options: any) => {
      queryKeyComputed = options.queryKey;
      return { data: ref(mockPaginatedEvents), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });

    useEventsQuery(filtersRef, { eventService: mockEventService });

    expect(queryKeyComputed?.value).toEqual(queryKeys.events.list(filtersRef.value));
  });
});
