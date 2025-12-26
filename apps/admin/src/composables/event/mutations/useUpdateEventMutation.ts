import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import type { UpdateEventDto } from '@/types';
import { type EventServiceAdapter, DefaultEventServiceAdapter } from '../event.adapter'; // Updated import

interface UseUpdateEventMutationDeps {
  eventService: EventServiceAdapter;
}

export function useUpdateEventMutation(
  deps: UseUpdateEventMutationDeps = { eventService: DefaultEventServiceAdapter }
) {
  const { eventService } = deps;
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (updatedEvent: UpdateEventDto) => {
      if (!updatedEvent.id) {
        throw new Error('EventDto ID is required for update');
      }
      const response = await eventService.update(updatedEvent); // Use injected service
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.events.all });
      queryClient.invalidateQueries({ queryKey: queryKeys.events.detail(variables.id!) });
    },
  });
}

export type UseUpdateEventMutationReturn = ReturnType<typeof useUpdateEventMutation>;
