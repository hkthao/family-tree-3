import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useEventQuery } from '@/composables/event/queries/useEventQuery';
import { useQuery } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import type { EventDto } from '@/types';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import { ref, type Ref } from 'vue';
import { EventType, CalendarType, RepeatRule } from '@/types';

// Mock the external dependencies
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
  exportEvents: vi.fn() as Mock,
  importEvents: vi.fn() as Mock,
  getEventsByMemberId: vi.fn() as Mock, // Add this new method
};

describe('useEventQuery', () => {
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
    eventMemberIds: ['member1'], // Changed from relatedMemberIds
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should call eventService.getById with the correct event ID in queryFn', async () => {
    const eventIdRef = ref('event1');

    // Mock useQuery to immediately execute queryFn
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn();
      return { data: ref(mockEvent), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });
    (mockEventService.getById as Mock).mockResolvedValue({ ok: true, value: mockEvent });

    useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(mockEventService.getById).toHaveBeenCalledWith('event1');
  });

  it('should return event data on successful query', async () => {
    const eventIdRef = ref('event1');
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn();
      return { data: ref(mockEvent), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });
    (mockEventService.getById as Mock).mockResolvedValue({ ok: true, value: mockEvent });

    const { event } = useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(event.value).toEqual(mockEvent);
  });

  it('should return isLoading true while fetching', () => {
    const eventIdRef = ref('event1');
    (useQuery as Mock).mockImplementation(() => {
      return { data: ref(null), isFetching: ref(true), error: ref(null), refetch: vi.fn() };
    });

    const { isLoading } = useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(isLoading.value).toBe(true);
  });

  it('should return error on failed query', async () => {
    const eventIdRef = ref('event1');
    const mockError = new Error('Failed to fetch event');
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError)); // Directly mock queryFn to return a rejected promise
      return { data: ref(null), isFetching: ref(false), error: ref(mockError), refetch: vi.fn() };
    });
    (mockEventService.getById as Mock).mockResolvedValue({ ok: false, error: mockError });

    const { error } = useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(error.value).toEqual(mockError);
  });

  it('should throw an error if event ID is missing in queryFn', async () => {
    const eventIdRef = ref(undefined);
    const mockError = new Error('EventDto ID is required');
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError));
      return {
        data: ref(null),
        isFetching: ref(false),
        error: ref(mockError),
        refetch: vi.fn(),
      };
    });

    const { query } = useEventQuery(eventIdRef, { eventService: mockEventService });
    
    expect(query.error.value?.message).toBe('EventDto ID is required');
  });

  it('should throw an error if event not found (response.value is undefined)', async () => {
    const eventIdRef = ref('event1');
    const mockError = new Error('EventDto not found');
    (useQuery as Mock).mockImplementation((options: any) => {
      options.queryFn = vi.fn(() => Promise.reject(mockError));
      return {
        data: ref(null),
        isFetching: ref(false),
        error: ref(mockError),
        refetch: vi.fn(),
      };
    });
    (mockEventService.getById as Mock).mockResolvedValue({ ok: true, value: undefined });

    const { query } = useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(query.error.value?.message).toBe('EventDto not found');
  });

  it('should set enabled to false if eventId is undefined', () => {
    const eventIdRef = ref(undefined);
    let enabledComputed: Ref<boolean> | undefined;

    (useQuery as Mock).mockImplementation((options: any) => {
      enabledComputed = options.enabled;
      return { data: ref(null), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });

    useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(enabledComputed?.value).toBe(false);
  });

  it('should set enabled to true if eventId is defined', () => {
    const eventIdRef = ref('event1');
    let enabledComputed: Ref<boolean> | undefined;

    (useQuery as Mock).mockImplementation((options: any) => {
      enabledComputed = options.enabled;
      return { data: ref(null), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });

    useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(enabledComputed?.value).toBe(true);
  });

  it('should have the correct queryKey', () => {
    const eventIdRef = ref('event1');
    let queryKeyComputed: Ref<any> | undefined;

    (useQuery as Mock).mockImplementation((options: any) => {
      queryKeyComputed = options.queryKey;
      return { data: ref(null), isFetching: ref(false), error: ref(null), refetch: vi.fn() };
    });

    useEventQuery(eventIdRef, { eventService: mockEventService });

    expect(queryKeyComputed?.value).toEqual(queryKeys.events.detail('event1'));
  });
});
