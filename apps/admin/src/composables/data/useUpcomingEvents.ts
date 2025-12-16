import { computed, unref, type Ref, type ComputedRef } from 'vue';
import { useEventsQuery } from '@/composables/event/useEventsQuery'; // Use the existing generic query
import type { EventFilter } from '@/types'; // Import EventFilter

export function useUpcomingEvents(_familyId: Ref<string | undefined> | ComputedRef<string | undefined>) {
  const upcomingEventsFilter = computed<EventFilter>(() => ({
    familyId: unref(_familyId),
    minSolarDate: new Date(), // Filter for events from today onwards
    itemsPerPage: 100, // Fetch a reasonable number of upcoming events
    sortBy: [{ key: 'solarDate', order: 'asc' }], // Correct sortBy type
  }));

  const {
    events: upcomingEvents, // Rename 'events' to 'upcomingEvents' for consistency
    loading, // Use 'loading' which maps to query.isFetching
    error, // Use 'error' from query
    refetch,
    query, // Keep query object to access isError if needed
  } = useEventsQuery(upcomingEventsFilter);

  return {
    upcomingEvents,
    isLoading: loading, // Map to isLoading for external consistency if needed
    isError: computed(() => query.isError.value), // Access isError from query
    error,
    isFetching: loading, // Map to isFetching for external consistency if needed
    refetch,
  };
}

