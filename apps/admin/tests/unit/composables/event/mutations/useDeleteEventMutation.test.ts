import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useDeleteEventMutation } from '@/composables/event/mutations/useDeleteEventMutation';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';

// Mock the external dependencies
vi.mock('@tanstack/vue-query', () => ({
  useMutation: vi.fn(),
  useQueryClient: vi.fn(),
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

// Mock queryClient
const mockQueryClient = {
  invalidateQueries: vi.fn(),
};

describe('useDeleteEventMutation', () => {
  const eventIdToDelete = 'event1';

  beforeEach(() => {
    vi.clearAllMocks();
    (useQueryClient as Mock).mockReturnValue(mockQueryClient);
  });

  it('should call eventService.delete with the correct event ID in mutationFn', async () => {
    // Mock useMutation to immediately execute mutationFn
    (useMutation as Mock).mockImplementation((options) => {
      options.mutationFn(eventIdToDelete);
      return {
        mutate: vi.fn(),
        isPending: false,
      };
    });
    (mockEventService.delete as Mock).mockResolvedValue({ ok: true });

    useDeleteEventMutation({ eventService: mockEventService });

    expect(mockEventService.delete).toHaveBeenCalledWith(eventIdToDelete);
  });

  it('should call onSuccess and invalidate queries on successful mutation', async () => {
    const onSuccessCallback = vi.fn();
    (useMutation as Mock).mockImplementation((options) => {
      options.onSuccess();
      return {
        mutate: vi.fn((data, callbacks) => callbacks.onSuccess()),
        isPending: false,
      };
    });
    (mockEventService.delete as Mock).mockResolvedValue({ ok: true });

    const { mutate } = useDeleteEventMutation({ eventService: mockEventService });
    mutate(eventIdToDelete, { onSuccess: onSuccessCallback });

    expect(onSuccessCallback).toHaveBeenCalled();
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.all });
  });

  it('should call onError on failed mutation', async () => {
    const onErrorCallback = vi.fn();
    const mockError = new Error('Failed to delete event');
    (useMutation as Mock).mockImplementation((options) => {
      options.mutationFn = vi.fn(() => Promise.reject(mockError));
      return {
        mutate: vi.fn((data, callbacks) => callbacks.onError(mockError)),
        isPending: false,
      };
    });
    (mockEventService.delete as Mock).mockResolvedValue({ ok: false, error: mockError });

    const { mutate } = useDeleteEventMutation({ eventService: mockEventService });
    mutate(eventIdToDelete, { onError: onErrorCallback });

    expect(onErrorCallback).toHaveBeenCalledWith(mockError);
    expect(mockQueryClient.invalidateQueries).not.toHaveBeenCalled();
  });
});
