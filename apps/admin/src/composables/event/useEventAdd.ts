import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddEventMutation } from '@/composables/event';

export function useEventAdd(emit: (event: 'saved' | 'close', ...args: any[]) => void) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { mutate: addEvent, isPending: isAddingEvent } = useAddEventMutation();

  const handleAddEvent = async (eventData: Omit<Event, 'id'>) => {
    addEvent(eventData, {
      onSuccess: () => {
        showSnackbar(t('event.messages.addSuccess'), 'success');
        emit('saved');
      },
      onError: (error) => {
        showSnackbar(error.message || t('event.messages.saveError'), 'error');
      },
    });
  };

  const closeForm = () => {
    emit('close');
  };

  return {
    isAddingEvent,
    handleAddEvent,
    closeForm,
    t, // Return t for use in the component template
  };
}
