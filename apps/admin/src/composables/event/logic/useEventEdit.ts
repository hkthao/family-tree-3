import { toRef, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventDto } from '@/types';
import { type UseGlobalSnackbarReturn } from '@/composables/ui/useGlobalSnackbar';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useEventQuery, type UseEventQueryReturn } from '@/composables/event/queries/useEventQuery';
import { useUpdateEventMutation, type UseUpdateEventMutationReturn } from '@/composables/event/mutations/useUpdateEventMutation';
import { useQueryClient, type QueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';

interface UseEventEditDeps {
  useI18n: typeof useI18n;
  useGlobalSnackbar: () => UseGlobalSnackbarReturn;
  useEventQuery: (eventId: Ref<string | undefined>) => UseEventQueryReturn;
  useUpdateEventMutation: () => UseUpdateEventMutationReturn;
  useQueryClient: () => QueryClient;
}

const defaultDeps: UseEventEditDeps = {
  useI18n,
  useGlobalSnackbar,
  useEventQuery,
  useUpdateEventMutation,
  useQueryClient,
};

export function useEventEdit(
  emit: (event: 'saved' | 'close', ...args: any[]) => void,
  eventId: string,
  deps: UseEventEditDeps = defaultDeps,
) {
  const {
    useI18n,
    useGlobalSnackbar,
    useEventQuery,
    useUpdateEventMutation,
    useQueryClient,
  } = deps;

  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const queryClient = useQueryClient();

  const eventIdRef = toRef({ eventId }, 'eventId');
  const { event: eventData, isLoading: isLoadingEvent } = useEventQuery(eventIdRef);
  const { mutate: updateEvent, isPending: isUpdatingEvent } = useUpdateEventMutation();

  const handleUpdateEvent = async (eventToUpdate: EventDto) => {
    if (!eventToUpdate.id) {
      showSnackbar(t('event.messages.saveError'), 'error');
      return;
    }

    updateEvent(eventToUpdate, {
      onSuccess: () => {
        showSnackbar(t('event.messages.updateSuccess'), 'success');
        emit('saved');
        queryClient.invalidateQueries({ queryKey: queryKeys.events.all });
        queryClient.invalidateQueries({ queryKey: queryKeys.events.detail(eventToUpdate.id!) });
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
      eventData,
      isLoadingEvent,
      isUpdatingEvent,
    },
    actions: {
      handleUpdateEvent,
      closeForm,
      t, // Return t for use in the component template
    },
  };
}