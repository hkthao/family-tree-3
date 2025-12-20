// tests/unit/composables/event/mutations/useUpdateEventMutation.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useQueryClient } from '@tanstack/vue-query';
import { useUpdateEventMutation } from '@/composables/event/mutations/useUpdateEventMutation';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import type { Event } from '@/types';
import { success, failure } from '@/utils/result';
import { queryKeys } from '@/constants/queryKeys';

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

describe('useUpdateEventMutation', () => {
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

  it('should call eventService.update and invalidate queries on success', async () => {
    const updatedEvent: Event = {
      id: 'event1',
      name: 'Updated Event',
      description: 'Updated Description',
      familyId: 'family1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date(),
      repeatRule: 'None',
      relatedMemberIds: [],
      color: '#000000',
    };
    vi.mocked(mockEventService.update).mockResolvedValue(success(updatedEvent));

    const { mutate } = useUpdateEventMutation({ eventService: mockEventService });

    await mutate(updatedEvent);

    expect(mockEventService.update).toHaveBeenCalledWith(updatedEvent);
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.all });
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.detail(updatedEvent.id) });
  });

  it('should throw an error if updatedEvent.id is missing', async () => {
    const updatedEvent: Omit<Event, 'id'> = {
      name: 'Updated Event',
      description: 'Updated Description',
      familyId: 'family1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date(),
      repeatRule: 'None',
      relatedMemberIds: [],
      color: '#000000',
    };
    vi.mocked(mockEventService.update).mockResolvedValue(success({ ...updatedEvent, id: 'event1' } as Event));

    const { mutate } = useUpdateEventMutation({ eventService: mockEventService });

    await expect(
      new Promise((resolve, reject) => {
        mutate(updatedEvent as Event, {
          onSuccess: resolve,
          onError: reject,
        });
      }),
    ).rejects.toThrow('Event ID is required for update');
    expect(mockEventService.update).not.toHaveBeenCalled();
    expect(mockQueryClient.invalidateQueries).not.toHaveBeenCalled();
  });

  it('should throw an error if eventService.update fails', async () => {
    const updatedEvent: Event = {
      id: 'event1',
      name: 'Updated Event',
      description: 'Updated Description',
      familyId: 'family1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date(),
      repeatRule: 'None',
      relatedMemberIds: [],
      color: '#000000',
    };
    const errorResult = failure(new Error('Failed to update event'));
    vi.mocked(mockEventService.update).mockResolvedValue(errorResult);

    const { mutate } = useUpdateEventMutation({ eventService: mockEventService });

    await expect(
      new Promise((resolve, reject) => {
        mutate(updatedEvent, {
          onSuccess: resolve,
          onError: reject,
        });
      }),
    ).rejects.toThrow('Failed to update event');
    expect(mockEventService.update).toHaveBeenCalledWith(updatedEvent);
    expect(mockQueryClient.invalidateQueries).not.toHaveBeenCalled();
  });

  it('should return isPending as false initially', () => {
    const { isPending } = useUpdateEventMutation({ eventService: mockEventService });
    expect(isPending).toBe(false);
  });
});
