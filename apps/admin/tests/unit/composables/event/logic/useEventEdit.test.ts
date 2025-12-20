// tests/unit/composables/event/logic/useEventEdit.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventEdit } from '@/composables/event/logic/useEventEdit';
import type { Event } from '@/types';
import type { UseGlobalSnackbarReturn } from '@/composables/common/useGlobalSnackbar';
import type { UseEventQueryReturn } from '@/composables/event/queries/useEventQuery';
import type { UseUpdateEventMutationReturn } from '@/composables/event/mutations/useUpdateEventMutation';
import { queryKeys } from '@/constants/queryKeys';

// Mock external dependencies
vi.mock('vue', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue')>();
  return {
    ...actual,
    toRef: vi.fn((obj, key) => ref(obj[key])), // Mock toRef for simplicity
  };
});
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(),
}));
vi.mock('@/composables', () => ({
  useGlobalSnackbar: vi.fn(),
}));
vi.mock('@/composables/event/queries/useEventQuery', () => ({
  useEventQuery: vi.fn(),
}));
vi.mock('@/composables/event/mutations/useUpdateEventMutation', () => ({
  useUpdateEventMutation: vi.fn(),
}));
vi.mock('@tanstack/vue-query', () => ({
  useQueryClient: vi.fn(() => ({
    invalidateQueries: vi.fn(),
  })),
}));

describe('useEventEdit', () => {
  let mockT: vi.Mock;
  let mockShowSnackbar: vi.Mock;
  let mockUseEventQuery: vi.Mock;
  let mockEventData: ReturnType<typeof ref>;
  let mockIsLoadingEvent: ReturnType<typeof ref>;
  let mockUpdateEventMutate: vi.Mock;
  let mockIsUpdatingEvent: ReturnType<typeof ref>;
  let mockInvalidateQueries: vi.Mock;
  let mockEmit: vi.Mock;

  const eventId = 'event123';
  const initialEvent: Event = {
    id: eventId,
    name: 'Initial Event',
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

    mockT = vi.fn((key) => key);
    vi.mocked(useI18n).mockReturnValue({ t: mockT } as any);

    mockShowSnackbar = vi.fn();
    vi.mocked(useGlobalSnackbar).mockReturnValue({ showSnackbar: mockShowSnackbar } as UseGlobalSnackbarReturn);

    mockEventData = ref(initialEvent);
    mockIsLoadingEvent = ref(false);
    mockUseEventQuery = vi.fn(() => ({
      event: mockEventData,
      isLoading: mockIsLoadingEvent,
    })) as unknown as typeof useEventQuery;
    vi.mocked(useEventQuery).mockReturnValue({ event: mockEventData, isLoading: mockIsLoadingEvent } as UseEventQueryReturn);

    mockUpdateEventMutate = vi.fn();
    mockIsUpdatingEvent = ref(false);
    vi.mocked(useUpdateEventMutation).mockReturnValue({
      mutate: mockUpdateEventMutate,
      isPending: mockIsUpdatingEvent,
    } as UseUpdateEventMutationReturn);

    mockInvalidateQueries = vi.mocked(useQueryClient().invalidateQueries);

    mockEmit = vi.fn();
  });

  it('should return initial state and actions', () => {
    const { state, actions } = useEventEdit(mockEmit, eventId);
    expect(state.eventData.value).toEqual(initialEvent);
    expect(state.isLoadingEvent.value).toBe(false);
    expect(state.isUpdatingEvent.value).toBe(false);
    expect(actions.handleUpdateEvent).toBeTypeOf('function');
    expect(actions.closeForm).toBeTypeOf('function');
    expect(actions.t).toBe(mockT);
  });

  describe('handleUpdateEvent', () => {
    const updatedEvent: Event = { ...initialEvent, name: 'Updated Event Name' };

    it('should call updateEvent and show success snackbar on success', async () => {
      mockUpdateEventMutate.mockImplementation((_data, callbacks) => {
        callbacks.onSuccess();
      });

      const { actions } = useEventEdit(mockEmit, eventId);
      await actions.handleUpdateEvent(updatedEvent);

      expect(mockUpdateEventMutate).toHaveBeenCalledWith(updatedEvent, expect.any(Object));
      expect(mockShowSnackbar).toHaveBeenCalledWith('event.messages.updateSuccess', 'success');
      expect(mockEmit).toHaveBeenCalledWith('saved');
      expect(mockInvalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.all });
      expect(mockInvalidateQueries).toHaveBeenCalledWith({ queryKey: queryKeys.events.detail(eventId) });
    });

    it('should call updateEvent and show error snackbar on error', async () => {
      const errorMessage = 'Failed to update';
      mockUpdateEventMutate.mockImplementation((_data, callbacks) => {
        callbacks.onError(new Error(errorMessage));
      });

      const { actions } = useEventEdit(mockEmit, eventId);
      await actions.handleUpdateEvent(updatedEvent);

      expect(mockUpdateEventMutate).toHaveBeenCalledWith(updatedEvent, expect.any(Object));
      expect(mockShowSnackbar).toHaveBeenCalledWith(errorMessage, 'error');
      expect(mockEmit).not.toHaveBeenCalledWith('saved');
      expect(mockInvalidateQueries).not.toHaveBeenCalled();
    });

    it('should use default error message if error.message is undefined', async () => {
      mockUpdateEventMutate.mockImplementation((_data, callbacks) => {
        callbacks.onError({}); // Error without a message
      });

      const { actions } = useEventEdit(mockEmit, eventId);
      await actions.handleUpdateEvent(updatedEvent);

      expect(mockShowSnackbar).toHaveBeenCalledWith('event.messages.saveError', 'error');
    });

    it('should show error snackbar and not call updateEvent if eventToUpdate.id is missing', async () => {
      const eventWithoutId = { ...updatedEvent, id: undefined } as unknown as Event;

      const { actions } = useEventEdit(mockEmit, eventId);
      await actions.handleUpdateEvent(eventWithoutId);

      expect(mockShowSnackbar).toHaveBeenCalledWith('event.messages.saveError', 'error');
      expect(mockUpdateEventMutate).not.toHaveBeenCalled();
      expect(mockEmit).not.toHaveBeenCalledWith('saved');
      expect(mockInvalidateQueries).not.toHaveBeenCalled();
    });
  });

  it('should emit "close" when closeForm is called', () => {
    const { actions } = useEventEdit(mockEmit, eventId);
    actions.closeForm();
    expect(mockEmit).toHaveBeenCalledWith('close');
  });
});
