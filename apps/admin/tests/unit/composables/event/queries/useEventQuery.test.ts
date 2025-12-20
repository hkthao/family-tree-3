// tests/unit/composables/event/queries/useEventQuery.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useEventQuery } from '@/composables/event/queries/useEventQuery';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import type { Event } from '@/types';
import { success, failure } from '@/utils/result';

// Mock useQuery
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
}));

describe('useEventQuery', () => {
  let mockEventService: EventServiceAdapter;
  const mockEvent: Event = {
    id: '1',
    name: 'Test Event',
    description: 'Description',
    familyId: 'family1',
    type: 'Other',
    calendarType: 'Solar',
    solarDate: new Date(),
    repeatRule: 'None',
    relatedMemberIds: [],
    color: '#000000',
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
      data: ref(mockEvent), // Default data
      isFetching: ref(false), // Default loading state
      error: ref(null), // Default error state
      refetch: vi.fn(),
      // Mock other properties useQuery might return if needed
    } as any);
  });

  it('should call eventService.getById with the correct eventId and return event', async () => {
    const eventIdRef = ref('1');
    vi.mocked(mockEventService.getById).mockResolvedValue(success(mockEvent));

    const { event, isLoading } = useEventQuery(eventIdRef, { eventService: mockEventService });

    // Manually trigger queryFn as useQuery is mocked
    const queryFn = vi.mocked(useQuery).mock.calls[0][0].queryFn;
    const result = await queryFn();

    expect(mockEventService.getById).toHaveBeenCalledWith('1');
    expect(result).toEqual(mockEvent);
    expect(event.value).toEqual(mockEvent);
    expect(isLoading.value).toBe(false);
  });

  it('should throw an error if eventId is undefined and queryFn is called', async () => {
    const eventIdRef = ref(undefined);
    const { query } = useEventQuery(eventIdRef, { eventService: mockEventService });

    const queryFn = vi.mocked(useQuery).mock.calls[0][0].queryFn;
    await expect(queryFn()).rejects.toThrow('Event ID is required');
    expect(mockEventService.getById).not.toHaveBeenCalled();
    expect(query.enabled.value).toBe(false); // Ensure query is disabled
  });

  it('should throw an error if eventService.getById fails', async () => {
    const eventIdRef = ref('1');
    const errorResult = failure(new Error('API Error'));
    vi.mocked(mockEventService.getById).mockResolvedValue(errorResult);

    useEventQuery(eventIdRef, { eventService: mockEventService });

    const queryFn = vi.mocked(useQuery).mock.calls[0][0].queryFn;
    await expect(queryFn()).rejects.toThrow('API Error');
  });

  it('should throw an error if eventService.getById returns undefined (event not found)', async () => {
    vi.mocked(mockEventService.getById).mockResolvedValue(success(undefined));
    const queryFn = vi.mocked(useQuery).mock.calls[0][0].queryFn;
    await expect(queryFn()).rejects.toThrow('Event not found');
  });

  it('should reflect loading state', () => {
    const eventIdRef = ref('1');
    vi.mocked(useQuery).mockReturnValue({
      data: ref(null),
      isFetching: ref(true), // Simulate loading
      error: ref(null),
      refetch: vi.fn(),
    } as any);

    const { isLoading } = useEventQuery(eventIdRef, { eventService: mockEventService });
    expect(isLoading.value).toBe(true);
  });
});
