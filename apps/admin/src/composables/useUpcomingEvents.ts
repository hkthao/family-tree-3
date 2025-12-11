import { useQuery } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError, Event, Result } from '@/types';
import { computed, type Ref, type ComputedRef } from 'vue';

export function useUpcomingEvents(familyId: Ref<string | undefined> | ComputedRef<string | undefined>) {
  const { event } = useServices();

  const query = useQuery<Event[] | undefined, ApiError>({
    queryKey: computed(() => queryKeys.events.upcoming(familyId.value)).value,
    queryFn: async () => {
      const result = await event.getUpcomingEvents(familyId.value);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    select: (data) => data,
    enabled: computed(() => !!event).value, // Ensure service is available
  });

  return {
    upcomingEvents: query.data,
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    isFetching: query.isFetching,
    refetch: query.refetch,
  };
}
