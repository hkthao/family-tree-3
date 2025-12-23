import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Event } from '@/types';
import { queryKeys } from '@/constants/queryKeys';
import { type EventServiceAdapter, DefaultEventServiceAdapter } from '../event.adapter'; // Updated import

interface UseEventQueryDeps {
  eventService: EventServiceAdapter;
}

export function useEventQuery(
  eventId: Ref<string | undefined>,
  deps: UseEventQueryDeps = { eventService: DefaultEventServiceAdapter }
) {
  const { eventService } = deps;
  const query = useQuery<Event, Error>({
    queryKey: computed(() => (unref(eventId) ? queryKeys.events.detail(unref(eventId)!) : [])),
    queryFn: async () => {
      const id = unref(eventId);
      if (!id) {
        throw new Error('Event ID is required');
      }
      const response = await eventService.getById(id); // Use injected service
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

export type UseEventQueryReturn = ReturnType<typeof useEventQuery>;
