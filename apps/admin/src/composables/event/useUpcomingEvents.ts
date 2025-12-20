import { computed, unref, type Ref, type ComputedRef } from 'vue';
import { useEventsQuery, type UseEventsQueryReturn } from '@/composables/event/queries/useEventsQuery'; // Updated import
import type { EventFilter } from '@/types';

interface UseUpcomingEventsDeps {
  useEventsQuery: (filters: Ref<EventFilter>) => UseEventsQueryReturn;
}

export function useUpcomingEvents(
  baseFilter: Ref<EventFilter> | ComputedRef<EventFilter>,
  deps: UseUpcomingEventsDeps = { useEventsQuery }
) {
  const upcomingEventsFilter = computed<EventFilter>(() => {
    const currentBaseFilter = unref(baseFilter);
    return {
      ...currentBaseFilter,
      itemsPerPage: 100, // Fetch a reasonable number of upcoming events
      sortBy: [{ key: 'solarDate', order: 'asc' }], // Correct sortBy type
    };
  });

  const {
    events: upcomingEvents, // Rename 'events' to 'upcomingEvents' for consistency
    loading, // Use 'loading' which maps to query.isFetching
    error, // Use 'error' from query
    refetch,
    query, // Keep query object to access isError if needed
  } = deps.useEventsQuery(upcomingEventsFilter); // Use injected useEventsQuery

  return {
    upcomingEvents,
    isLoading: loading, // Map to isLoading for external consistency if needed
    isError: computed(() => query.isError.value), // Access isError from query
    error,
    isFetching: loading, // Map to isFetching for external consistency if needed
    refetch,
  };
}

export type UseUpcomingEventsReturn = ReturnType<typeof useUpcomingEvents>;


