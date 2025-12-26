import { useI18n } from 'vue-i18n';
import type { AddEventDto } from '@/types';
import { type UseGlobalSnackbarReturn } from '@/composables/ui/useGlobalSnackbar';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useAddEventMutation, type UseAddEventMutationReturn } from '@/composables/event/mutations/useAddEventMutation';

interface UseEventAddDeps {
  useI18n: typeof useI18n;
  useGlobalSnackbar: () => UseGlobalSnackbarReturn;
  useAddEventMutation: () => UseAddEventMutationReturn;
}

const defaultDeps: UseEventAddDeps = {
  useI18n,
  useGlobalSnackbar,
  useAddEventMutation,
};

export function useEventAdd(
  emit: (event: 'saved' | 'close', ...args: any[]) => void,
  deps: UseEventAddDeps = defaultDeps,
) {
  const {
    useI18n,
    useGlobalSnackbar,
    useAddEventMutation,
  } = deps;

  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { mutate: addEvent, isPending: isAddingEvent } = useAddEventMutation();

  const handleAddEvent = async (eventData: AddEventDto) => {
    addEvent(eventData, {
      onSuccess: () => {
        showSnackbar(t('event.messages.addSuccess'), 'success');
        emit('saved');
      },
      onError: (error: Error) => {
        showSnackbar(error.message || t('event.messages.saveError'), 'error');
      },
    });
  };

  const closeForm = () => {
    emit('close');
  };

  return {
    state: {
      isAddingEvent,
    },
    actions: {
      handleAddEvent,
      closeForm,
      t, // Return t for use in the component template
    },
  };
}
