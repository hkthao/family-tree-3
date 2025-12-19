import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ApiEventService } from '@/services/event/api.event.service';
import type { IEventService } from '@/services/event/event.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiEventService: IEventService = new ApiEventService(apiClient);

export function useDeleteEventMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (eventId: string) => {
      const response = await apiEventService.delete(eventId);
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
