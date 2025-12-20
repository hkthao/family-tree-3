import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { type EventServiceAdapter, DefaultEventServiceAdapter } from '../event.adapter';

interface UseDeleteEventMutationDeps {
  eventService: EventServiceAdapter;
}

export function useDeleteEventMutation(
  deps: UseDeleteEventMutationDeps = { eventService: DefaultEventServiceAdapter }
) {
  const { eventService } = deps;
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (eventId: string) => {
      const response = await eventService.delete(eventId);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.events.all });
    },
  });
}

export type UseDeleteEventMutationReturn = ReturnType<typeof useDeleteEventMutation>;
