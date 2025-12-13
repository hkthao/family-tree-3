import { toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useEventQuery, useUpdateEventMutation } from '@/composables/event';
import { useQueryClient } from '@tanstack/vue-query';

export function useEventEdit(
  emit: (event: 'saved' | 'close', ...args: any[]) => void,
  eventId: string,
) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient();

  const eventIdRef = toRef({ eventId }, 'eventId');
  const { event: eventData, isLoading: isLoadingEvent } = useEventQuery(eventIdRef);
  const { mutate: updateEvent, isPending: isUpdatingEvent } = useUpdateEventMutation();

  const handleUpdateEvent = async (eventToUpdate: Event) => {
    if (!eventToUpdate.id) {
      showSnackbar(t('event.messages.saveError'), 'error');
      return;
    }

    updateEvent(eventToUpdate, {
      onSuccess: () => {
        showSnackbar(t('event.messages.updateSuccess'), 'success');
        emit('saved');
        queryClient.invalidateQueries({ queryKey: ['events', 'detail', eventToUpdate.id] });
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
    eventData,
    isLoadingEvent,
    isUpdatingEvent,
    handleUpdateEvent,
    closeForm,
    t,
  };
}