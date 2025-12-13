import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ApiEventService } from '@/services/event/api.event.service';
import type { IEventService } from '@/services/event/event.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';
import type { Event } from '@/types';

const apiEventService: IEventService = new ApiEventService(apiClient);

export function useAddEventMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (eventData: Omit<Event, 'id'>) => {
      const response = await apiEventService.add(eventData);
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
