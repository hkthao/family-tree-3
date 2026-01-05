import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { useAddEventMutation } from '@/composables/event/mutations/useAddEventMutation';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import type { EventDto } from '@/types';
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
  exportEvents: vi.fn(),
  importEvents: vi.fn(),
  getEventsByMemberId: vi.fn(), // Add this
};

// Mock queryClient
const mockQueryClient = {
  invalidateQueries: vi.fn(),
};

describe('useAddEventMutation', () => {
  const mockEventData: Omit<EventDto, 'id'> = {
    name: 'Test EventDto',
    code: 'TE001',
    type: 0, // EventType.Other
    familyId: 'family1',
    calendarType: 1, // CalendarType.Solar
    solarDate: new Date('2023-01-01'),
    lunarDate: null,
    repeatRule: 0, // RepeatRule.None
    description: 'A test description',
    color: '#FF0000',
    eventMemberIds: ['member1'], // Changed from relatedMemberIds
  };
  const mockEvent: EventDto = { id: 'event1', ...mockEventData };

  beforeEach(() => {
    vi.clearAllMocks();
    (useQueryClient as Mock).mockReturnValue(mockQueryClient);
  });

  it('should call eventService.add with correct data in mutationFn', async () => {
    // Mock useMutation to immediately execute mutationFn
    (useMutation as Mock).mockImplementation((options) => {
      options.mutationFn(mockEventData);
      return {
        mutate: vi.fn(),
        isPending: false,
      };
    });
    (mockEventService.add as Mock).mockResolvedValue({ ok: true, value: mockEvent });

    useAddEventMutation({ eventService: mockEventService });

    expect(mockEventService.add).toHaveBeenCalledWith(mockEventData);
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
    (mockEventService.add  as Mock).mockResolvedValue({ ok: true, value: mockEvent });

    const { mutate } = useAddEventMutation({ eventService: mockEventService });
    mutate(mockEventData, { onSuccess: onSuccessCallback });

    expect(onSuccessCallback).toHaveBeenCalled();
    expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.all });
  });

  it('should call onError on failed mutation', async () => {
    const onErrorCallback = vi.fn();
    const mockError = new Error('Failed to add event');
    (useMutation as Mock).mockImplementation((options) => {
      options.mutationFn = vi.fn(() => Promise.reject(mockError));
      return {
        mutate: vi.fn((data, callbacks) => callbacks.onError(mockError)),
        isPending: false,
      };
    });
    (mockEventService.add as Mock).mockResolvedValue({ ok: false, error: mockError });

    const { mutate } = useAddEventMutation({ eventService: mockEventService });
    mutate(mockEventData, { onError: onErrorCallback });

    expect(onErrorCallback).toHaveBeenCalledWith(mockError);
    expect(mockQueryClient.invalidateQueries).not.toHaveBeenCalled();
  });
});
