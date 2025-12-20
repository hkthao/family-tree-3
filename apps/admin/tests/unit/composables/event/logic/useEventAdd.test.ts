// tests/unit/composables/event/logic/useEventAdd.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useEventAdd } from '@/composables/event/logic/useEventAdd';
import type { Event } from '@/types';
import type { UseGlobalSnackbarReturn } from '@/composables/common/useGlobalSnackbar';
import type { UseAddEventMutationReturn } from '@/composables/event/mutations/useAddEventMutation';

// Mock external dependencies
vi.mock('vue-i18n', () => ({
  useI18n: vi.fn(),
}));
vi.mock('@/composables', () => ({
  useGlobalSnackbar: vi.fn(),
}));
vi.mock('@/composables/event/mutations/useAddEventMutation', () => ({
  useAddEventMutation: vi.fn(),
}));

describe('useEventAdd', () => {
  let mockT: vi.Mock;
  let mockShowSnackbar: vi.Mock;
  let mockAddEventMutate: vi.Mock;
  let mockIsAddingEvent: ReturnType<typeof ref>;
  let mockEmit: vi.Mock;

  beforeEach(() => {
    vi.clearAllMocks();

    mockT = vi.fn((key) => key);
    vi.mocked(useI18n).mockReturnValue({ t: mockT } as any);

    mockShowSnackbar = vi.fn();
    vi.mocked(useGlobalSnackbar).mockReturnValue({ showSnackbar: mockShowSnackbar } as UseGlobalSnackbarReturn);

    mockAddEventMutate = vi.fn();
    mockIsAddingEvent = ref(false);
    vi.mocked(useAddEventMutation).mockReturnValue({
      mutate: mockAddEventMutate,
      isPending: mockIsAddingEvent,
    } as UseAddEventMutationReturn);

    mockEmit = vi.fn();
  });

  it('should return initial state and actions', () => {
    const { state, actions } = useEventAdd(mockEmit);
    expect(state.isAddingEvent.value).toBe(false);
    expect(actions.handleAddEvent).toBeTypeOf('function');
    expect(actions.closeForm).toBeTypeOf('function');
    expect(actions.t).toBe(mockT);
  });

  describe('handleAddEvent', () => {
    const eventData: Omit<Event, 'id'> = {
      name: 'New Event',
      description: 'Test',
      familyId: '1',
      type: 'Other',
      calendarType: 'Solar',
      solarDate: new Date(),
      repeatRule: 'None',
      relatedMemberIds: [],
      color: '#000000',
    };

    it('should call addEvent and show success snackbar on success', async () => {
      mockAddEventMutate.mockImplementation((_data, callbacks) => {
        callbacks.onSuccess();
      });

      const { actions } = useEventAdd(mockEmit);
      await actions.handleAddEvent(eventData);

      expect(mockAddEventMutate).toHaveBeenCalledWith(eventData, expect.any(Object));
      expect(mockShowSnackbar).toHaveBeenCalledWith('event.messages.addSuccess', 'success');
      expect(mockEmit).toHaveBeenCalledWith('saved');
    });

    it('should call addEvent and show error snackbar on error', async () => {
      const errorMessage = 'Failed to add';
      mockAddEventMutate.mockImplementation((_data, callbacks) => {
        callbacks.onError(new Error(errorMessage));
      });

      const { actions } = useEventAdd(mockEmit);
      await actions.handleAddEvent(eventData);

      expect(mockAddEventMutate).toHaveBeenCalledWith(eventData, expect.any(Object));
      expect(mockShowSnackbar).toHaveBeenCalledWith(errorMessage, 'error');
      expect(mockEmit).not.toHaveBeenCalledWith('saved');
    });

    it('should use default error message if error.message is undefined', async () => {
      mockAddEventMutate.mockImplementation((_data, callbacks) => {
        callbacks.onError({}); // Error without a message
      });

      const { actions } = useEventAdd(mockEmit);
      await actions.handleAddEvent(eventData);

      expect(mockShowSnackbar).toHaveBeenCalledWith('event.messages.saveError', 'error');
    });
  });

  it('should emit "close" when closeForm is called', () => {
    const { actions } = useEventAdd(mockEmit);
    actions.closeForm();
    expect(mockEmit).toHaveBeenCalledWith('close');
  });
});
