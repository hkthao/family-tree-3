import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ApiEventService } from '@/services/event/api.event.service';
import type { IEventService } from '@/services/event/event.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';
import type { Event } from '@/types';

const apiEventService: IEventService = new ApiEventService(apiClient);

export function useUpdateEventMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (updatedEvent: Event) => {
      if (!updatedEvent.id) {
        throw new Error('Event ID is required for update');
      }
      const response = await apiEventService.update(updatedEvent);
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
