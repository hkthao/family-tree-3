import { computed, type Ref, type ComputedRef } from 'vue';
// import { useUpcomingEventsQuery } from '@/composables/event/useEventsQuery'; // Temporarily commented out

export function useUpcomingEvents(_familyId: Ref<string | undefined> | ComputedRef<string | undefined>) {
  // const {
  //   upcomingEvents,
  //   isLoading,
  //   isError,
  //   error,
  //   isFetching,
  //   refetch,
  // } = useUpcomingEventsQuery(computed(() => familyId.value).value);

  // return {
  //   upcomingEvents,
  //   isLoading,
  //   isError,
  //   error,
  //   isFetching,
  //   refetch,
  // };

  // Placeholder return for now
  return {
    upcomingEvents: computed(() => []),
    isLoading: computed(() => false),
    isError: computed(() => false),
    error: computed(() => null),
    isFetching: computed(() => false),
    refetch: () => {},
  };
}

