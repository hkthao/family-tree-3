import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import type { AddEventDto } from '@/types';
import { type EventServiceAdapter, DefaultEventServiceAdapter } from '../event.adapter'; // Updated import

interface UseAddEventMutationDeps {
  eventService: EventServiceAdapter;
}

export function useAddEventMutation(
  deps: UseAddEventMutationDeps = { eventService: DefaultEventServiceAdapter }
) {
  const { eventService } = deps;
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (eventData: AddEventDto) => {
      const response = await eventService.add(eventData); // Use injected service
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

export type UseAddEventMutationReturn = ReturnType<typeof useAddEventMutation>;
