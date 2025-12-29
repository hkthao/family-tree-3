import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useUpdateEventMutation } from '@/composables/event/mutations/useUpdateEventMutation';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import type { EventDto } from '@/types';
import type { EventServiceAdapter } from '@/composables/event/event.adapter';
import { EventType, CalendarType, RepeatRule } from '@/types'; // Import necessary enums

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
  exportEvents: vi.fn(),
  importEvents: vi.fn(),
};

// Mock queryClient
const mockQueryClient = {
  invalidateQueries: vi.fn(),
};

describe('useUpdateEventMutation', () => {
  const mockEvent: EventDto = {
    id: 'event1',
    name: 'Updated EventDto',
    code: 'UE001',
    type: EventType.Other,
    familyId: 'family1',
    calendarType: CalendarType.Solar,
    solarDate: new Date('2023-01-01'),
    lunarDate: null,
    repeatRule: RepeatRule.None,
    description: 'An updated description',
    color: '#0000FF',
    relatedMemberIds: ['member1'],
  };

  beforeEach(() => {
    vi.clearAllMocks();
    (useQueryClient as Mock).mockReturnValue(mockQueryClient);
  });

  it('should call eventService.update with correct data in mutationFn', async () => {
    // Mock useMutation to immediately execute mutationFn
    (useMutation as Mock).mockImplementation((options) => {
      options.mutationFn(mockEvent);
      return {
        mutate: vi.fn(),
        isPending: false,
      };
    });
    (mockEventService.update as Mock).mockResolvedValue({ ok: true, value: mockEvent });

    useUpdateEventMutation({ eventService: mockEventService });

    expect(mockEventService.update).toHaveBeenCalledWith(mockEvent);
  });

  it('should throw an error if event ID is missing', async () => {
    const eventWithoutId = { ...mockEvent, id: undefined as any };

    // Mock useMutation to allow testing the mutationFn's error path
    (useMutation as Mock).mockImplementation((options) => {
      return {
        mutate: vi.fn(async (data, callbacks) => {
          try {
            return await options.mutationFn(data); // Return the promise from mutationFn
          } catch (error) {
            callbacks.onError(error);
            throw error; // Re-throw the error so .rejects can catch it
          }
        }),
        isPending: false,
      };
    });

    const { mutate } = useUpdateEventMutation({ eventService: mockEventService });

    await expect(
      mutate(eventWithoutId, { onError: (error) => expect(error.message).toBe('EventDto ID is required for update') }),
    ).rejects.toThrow('EventDto ID is required for update');
  });

  it('should call onSuccess and invalidate queries on successful mutation', async () => {
    const onSuccessCallback = vi.fn();
    (useMutation as Mock).mockImplementation((options) => {
      options.onSuccess(mockEvent, mockEvent); // pass data and variables
      return {
        mutate: vi.fn((data, callbacks) => callbacks.onSuccess(data, data)),
        isPending: false,
      };
    });
    (mockEventService.update as Mock).mockResolvedValue({ ok: true, value: mockEvent });

    const { mutate } = useUpdateEventMutation({ eventService: mockEventService });
    mutate(mockEvent, { onSuccess: onSuccessCallback });

    expect(onSuccessCallback).toHaveBeenCalled();
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.all });
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.detail(mockEvent.id) });
  });

  it('should call onError on failed mutation', async () => {
    const onErrorCallback = vi.fn();
    const mockError = new Error('Failed to update event');
    (useMutation as Mock).mockImplementation((options) => {
      options.mutationFn = vi.fn(() => Promise.reject(mockError));
      return {
        mutate: vi.fn((data, callbacks) => callbacks.onError(mockError)),
        isPending: false,
      };
    });
    (mockEventService.update as Mock).mockResolvedValue({ ok: false, error: mockError });

    const { mutate } = useUpdateEventMutation({ eventService: mockEventService });
    mutate(mockEvent, { onError: onErrorCallback });

    expect(onErrorCallback).toHaveBeenCalledWith(mockError);
    expect(mockQueryClient.invalidateQueries).not.toHaveBeenCalled();
  });
});
