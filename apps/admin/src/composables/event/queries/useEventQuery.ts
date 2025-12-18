import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Event } from '@/types';
import { ApiEventService } from '@/services/event/api.event.service';
import type { IEventService } from '@/services/event/event.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiEventService: IEventService = new ApiEventService(apiClient);

export function useEventQuery(eventId: Ref<string | undefined>) {
  const query = useQuery<Event, Error>({
    queryKey: computed(() => (unref(eventId) ? queryKeys.events.detail(unref(eventId)!) : [])),
    queryFn: async () => {
      const id = unref(eventId);
      if (!id) {
        throw new Error('Event ID is required');
      }
      const response = await apiEventService.getById(id);
      if (response.ok) {
        if (response.value === undefined) {
          throw new Error('Event not found');
        }
        return response.value;
      }
      throw response.error;
    },
    enabled: computed(() => !!unref(eventId)),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const event = computed(() => query.data.value);
  const isLoading = computed(() => query.isFetching.value);

  return {
    query,
    event,
    isLoading,
    error: query.error,
    refetch: query.refetch,
  };
}
