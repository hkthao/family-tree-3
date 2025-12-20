// tests/unit/composables/event/mutations/useAddEventMutation.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useQueryClient } from '@tanstack/vue-query';
import { useAddEventMutation } from '@/composables/event/mutations/useAddEventMutation';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import type { Event } from '@/types';
import { success, failure } from '@/utils/result';

// Mock useQueryClient
vi.mock('@tanstack/vue-query', () => ({
  useQueryClient: vi.fn(),
  useMutation: vi.fn((options) => {
    // Mimic the actual behavior of useMutation for mutationFn and onSuccess
    return {
      mutate: (variables: any, callbacks?: any) => {
        options.mutationFn(variables).then((data: any) => {
          options.onSuccess(data, variables);
          if (callbacks?.onSuccess) {
            callbacks.onSuccess(data, variables);
          }
        }).catch((error: any) => {
          if (callbacks?.onError) {
            callbacks.onError(error, variables);
          }
        });
      },
      isPending: false, // Mock as not pending for simplicity in these tests
    };
  }),
}));

describe('useAddEventMutation', () => {
  let mockEventService: EventServiceAdapter;
  let mockQueryClient: ReturnType<typeof useQueryClient>;

  beforeEach(() => {
    vi.clearAllMocks();

    mockEventService = {
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getById: vi.fn(),
      search: vi.fn(),
    };

    mockQueryClient = {
      invalidateQueries: vi.fn(),
      // Add other methods if they are called in the composable and need mocking
    } as unknown as ReturnType<typeof useQueryClient>;

    vi.mocked(useQueryClient).mockReturnValue(mockQueryClient);
  });

  it('should call eventService.add and invalidate queries on success', async () => {
    const newEvent: Omit<Event, 'id'> = {
      name: 'New Event',
      description: 'Description',
      familyId: 'family1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date(),
      repeatRule: 'None',
      relatedMemberIds: [],
      color: '#000000',
    };
    const addedEvent: Event = { ...newEvent, id: 'event1' };
    vi.mocked(mockEventService.add).mockResolvedValue(success(addedEvent));

    const { mutate } = useAddEventMutation({ eventService: mockEventService });

    await mutate(newEvent);

    expect(mockEventService.add).toHaveBeenCalledWith(newEvent);
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: ['events'] });
  });

  it('should throw an error if eventService.add fails', async () => {
    const newEvent: Omit<Event, 'id'> = {
      name: 'New Event',
      description: 'Description',
      familyId: 'family1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date(),
      repeatRule: 'None',
      relatedMemberIds: [],
      color: '#000000',
    };
    const errorResult = failure(new Error('Failed to add event'));
    vi.mocked(mockEventService.add).mockResolvedValue(errorResult);

    const { mutate } = useAddEventMutation({ eventService: mockEventService });

    await expect(
      new Promise((resolve, reject) => {
        mutate(newEvent, {
          onSuccess: resolve,
          onError: reject,
        });
      }),
    ).rejects.toThrow('Failed to add event');
    expect(mockEventService.add).toHaveBeenCalledWith(newEvent);
    expect(mockQueryClient.invalidateQueries).not.toHaveBeenCalled();
  });

  it('should return isPending as false initially', () => {
    const { isPending } = useAddEventMutation({ eventService: mockEventService });
    expect(isPending).toBe(false);
  });
});
