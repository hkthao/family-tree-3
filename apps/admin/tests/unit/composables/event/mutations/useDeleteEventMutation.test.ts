// tests/unit/composables/event/mutations/useDeleteEventMutation.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useQueryClient } from '@tanstack/vue-query';
import { useDeleteEventMutation } from '@/composables/event/mutations/useDeleteEventMutation';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import type { Result } from '@/types';
import { success, failure } from '@/utils/result';
import { queryKeys } from '@/constants/queryKeys';

// Mock @/utils/result
vi.mock('@/utils/result', () => ({
  success: (value: any) => ({ ok: true, value }),
  failure: (error: any) => ({ ok: false, error }),
}));

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

describe('useDeleteEventMutation', () => {
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
    } as unknown as ReturnType<typeof useQueryClient>;

    vi.mocked(useQueryClient).mockReturnValue(mockQueryClient);
  });

  it('should call eventService.delete and invalidate queries on success', async () => {
    const eventId = 'event1';
    vi.mocked(mockEventService.delete).mockResolvedValue(success(undefined));

    const { mutate } = useDeleteEventMutation({ eventService: mockEventService });

    await mutate(eventId);

    expect(mockEventService.delete).toHaveBeenCalledWith(eventId);
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.all });
  });

  it('should throw an error if eventService.delete fails', async () => {
    const eventId = 'event1';
    const errorResult = failure(new Error('Failed to delete event'));
    vi.mocked(mockEventService.delete).mockResolvedValue(errorResult);

    const { mutate } = useDeleteEventMutation({ eventService: mockEventService });

    await expect(
      new Promise((resolve, reject) => {
        mutate(eventId, {
          onSuccess: resolve,
          onError: reject,
        });
      }),
    ).rejects.toThrow('Failed to delete event');
    expect(mockEventService.delete).toHaveBeenCalledWith(eventId);
    expect(mockQueryClient.invalidateQueries).not.toHaveBeenCalled();
  });

  it('should return isPending as false initially', () => {
    const { isPending } = useDeleteEventMutation({ eventService: mockEventService });
    expect(isPending).toBe(false);
  });
});
