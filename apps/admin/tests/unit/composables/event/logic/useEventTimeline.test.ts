// tests/unit/composables/event/logic/useEventTimeline.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref, computed, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQuery } from '@tanstack/vue-query';
import { useEventTimeline } from '@/composables/event/logic/useEventTimeline';
import type { Event, EventFilter, Paginated, ListOptions } from '@/types';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import * as eventTimelineLogic from '@/composables/event/logic/eventTimeline.logic';
import { success, failure } from '@/utils/result';

// Mock Vue hooks
vi.mock('vue', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue')>();
  return {
    ...actual,
    onMounted: vi.fn(),
    watch: vi.fn(),
  };
});
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(() => ({ t: vi.fn((key) => key) })),
}));
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
}));
vi.mock('@/utils/dateUtils', () => ({
  formatDate: vi.fn((date) => `formatted-${date}`),
}));

// Mock logic functions
vi.mock('@/composables/event/logic/eventTimeline.logic', () => ({
  mapFiltersToQueryOptions: vi.fn(),
  sortEventsBySolarDateDesc: vi.fn((data) => ({ ...data, items: data.items.reverse() })), // Simple mock sort
}));

describe('useEventTimeline', () => {
  let mockT: vi.Mock;
  let mockEventService: EventServiceAdapter;
  let mockUseQuery: vi.Mock;
  let mockFormatDate: vi.Mock;
  let mockRefetch: vi.Mock;

  const mockPaginatedEvents: Paginated<Event> = {
    items: [{
      id: '1',
      name: 'Event 1',
      solarDate: new Date('2024-01-01'),
      familyId: 'f1', type: 'Other', calendarType: 'Solar', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000',
    }, {
      id: '2',
      name: 'Event 2',
      solarDate: new Date('2024-01-02'),
      familyId: 'f1', type: 'Other', calendarType: 'Solar', repeatRule: 'None', description: '', relatedMemberIds: [], color: '#000000',
    }],
    totalItems: 2,
    totalPages: 1,
    pageNumber: 1,
    pageSize: 10,
    hasPreviousPage: false,
    hasNextPage: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();

    mockT = vi.mocked(useI18n().t);
    mockFormatDate = vi.mocked(formatDate);

    mockEventService = {
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getById: vi.fn(),
      search: vi.fn(),
    };

    mockRefetch = vi.fn();
    mockUseQuery = vi.fn(() => ({
      data: ref(mockPaginatedEvents),
      isLoading: ref(false),
      error: ref(null),
      refetch: mockRefetch,
    }));
    vi.mocked(useQuery).mockImplementation(mockUseQuery);

    vi.mocked(eventTimelineLogic.mapFiltersToQueryOptions).mockReturnValue({
      listOptions: { page: 1, itemsPerPage: 10, sortBy: [] },
      filterOptions: {},
    });
  });

  it('should initialize with default state', () => {
    const { state } = useEventTimeline({}, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });
    expect(state.selectedEventId.value).toBeNull();
    expect(state.detailDrawer.value).toBe(false);
    expect(state.list.value.items).toEqual(mockPaginatedEvents.items.reverse()); // Because of mock sortEventsBySolarDateDesc
    expect(state.isLoading.value).toBe(false);
  });

  it('should call useQuery with correct options', () => {
    const props = { familyId: 'f1', memberId: 'm1' };
    useEventTimeline(props, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });

    expect(mockUseQuery).toHaveBeenCalledTimes(1);
    const queryOptions = mockUseQuery.mock.calls[0][0];

    expect(queryOptions.queryKey.value).toEqual([queryKeys.events.list(expect.any(Object)), 1, 10, []]);
    expect(queryOptions.enabled.value).toBe(true);
    expect(queryOptions.staleTime).toBe(5 * 60 * 1000);
    expect(queryOptions.placeholderData).toEqual({ items: [], totalPages: 0, totalCount: 0 });
    expect(queryOptions.select).toBe(eventTimelineLogic.sortEventsBySolarDateDesc);
  });

  it('should call eventService.search and process result in queryFn', async () => {
    const props = { familyId: 'f1' };
    const { state } = useEventTimeline(props, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });

    mockEventService.search.mockResolvedValue(success(mockPaginatedEvents));

    const queryOptions = mockUseQuery.mock.calls[0][0];
    const queryFn = queryOptions.queryFn;

    const result = await queryFn();

    expect(eventTimelineLogic.mapFiltersToQueryOptions).toHaveBeenCalledWith(
      state.list.value.filters,
      1, // currentPage.value
      10, // itemsPerPage.value
      [] // sortBy.value
    );
    expect(mockEventService.search).toHaveBeenCalledWith(expect.any(Object), expect.any(Object));
    expect(result).toEqual(mockPaginatedEvents);
  });

  it('should throw error if eventService.search fails', async () => {
    const props = { familyId: 'f1' };
    useEventTimeline(props, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });

    mockEventService.search.mockResolvedValue(failure(new Error('API Error')));

    const queryOptions = mockUseQuery.mock.calls[0][0];
    const queryFn = queryOptions.queryFn;

    await expect(queryFn()).rejects.toThrow('API Error');
  });

  it('should update selectedEventId and open detailDrawer on showEventDetails', () => {
    const { actions, state } = useEventTimeline({}, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });
    actions.showEventDetails(mockPaginatedEvents.items[0]);
    expect(state.selectedEventId.value).toBe(mockPaginatedEvents.items[0].id);
    expect(state.detailDrawer.value).toBe(true);
  });

  it('should reset selectedEventId and close detailDrawer on handleDetailClosed', () => {
    const { actions, state } = useEventTimeline({}, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });
    state.selectedEventId.value = 'some-id';
    state.detailDrawer.value = true;

    actions.handleDetailClosed();
    expect(state.selectedEventId.value).toBeNull();
    expect(state.detailDrawer.value).toBe(false);
  });

  it('should update pagination options on setListOptions', () => {
    const { actions, state } = useEventTimeline({}, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });

    actions.setListOptions({ page: 2, itemsPerPage: 20 });
    // Since currentPage, itemsPerPage, sortBy are internal refs to useQuery,
    // we need to access them via the arguments passed to useQuery.
    const queryOptions = mockUseQuery.mock.calls[0][0];
    expect(queryOptions.queryKey.value[1]).toBe(2); // currentPage
    expect(queryOptions.queryKey.value[2]).toBe(20); // itemsPerPage
  });

  it('should update filters, reset page, and refetch on setFilters', () => {
    const { actions } = useEventTimeline({}, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });
    const newFilters: EventFilter = { familyId: 'f2' };
    actions.setFilters(newFilters);

    const queryOptions = mockUseQuery.mock.calls[0][0];
    expect(queryOptions.queryKey.value[0].familyId).toBe('f2');
    expect(queryOptions.queryKey.value[1]).toBe(1); // currentPage should be reset
    expect(mockRefetch).toHaveBeenCalledTimes(1);
  });

  it('should call setListOptions on handlePageChange', () => {
    const { actions } = useEventTimeline({}, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });
    actions.handlePageChange(3);
    const queryOptions = mockUseQuery.mock.calls[0][0];
    expect(queryOptions.queryKey.value[1]).toBe(3); // currentPage
  });

  it('should watch props.familyId and props.memberId and call setFilters', () => {
    const props = ref({ familyId: 'f1', memberId: 'm1' });
    useEventTimeline(props.value, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });

    expect(onMounted).toHaveBeenCalledTimes(1);
    vi.mocked(onMounted).mock.calls[0][0](); // Trigger onMounted to run initial setFilters
    expect(eventTimelineLogic.mapFiltersToQueryOptions).toHaveBeenCalledTimes(1); // Triggered by initial useQuery execution

    // Simulate watch callback
    expect(watch).toHaveBeenCalledTimes(1);
    const watchCallback = vi.mocked(watch).mock.calls[0][1];
    watchCallback(['f2', 'm2'], ['f1', 'm1']);

    const queryOptions = mockUseQuery.mock.calls[0][0];
    // Check if filters were updated in useQuery's queryKey
    expect(queryOptions.queryKey.value[0].familyId).toBe('f2');
    expect(queryOptions.queryKey.value[0].memberId).toBe('m2');
    expect(mockRefetch).toHaveBeenCalledTimes(2); // Initial + watch trigger
  });

  it('should expose formatDate', () => {
    const { actions } = useEventTimeline({}, {
      useI18n: useI18n,
      eventService: mockEventService,
      useQuery: mockUseQuery,
      formatDate: mockFormatDate,
    });
    actions.formatDate(new Date());
    expect(mockFormatDate).toHaveBeenCalledTimes(1);
  });
});
