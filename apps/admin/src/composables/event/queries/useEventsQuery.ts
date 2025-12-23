import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Event, EventFilter, Paginated, ListOptions, FilterOptions } from '@/types';
import { queryKeys } from '@/constants/queryKeys';
import { type EventServiceAdapter, DefaultEventServiceAdapter } from '../event.adapter'; // Updated import

interface UseEventsQueryDeps {
  eventService: EventServiceAdapter;
}

export function useEventsQuery(
  filters: Ref<EventFilter>,
  deps: UseEventsQueryDeps = { eventService: DefaultEventServiceAdapter }
) {
  const { eventService } = deps;
  const query = useQuery<Paginated<Event>, Error>({
    queryKey: computed(() => queryKeys.events.list(unref(filters))),
    queryFn: async () => {
      const currentFilters = unref(filters);

      const listOptions: ListOptions = {
        page: currentFilters.page,
        itemsPerPage: currentFilters.itemsPerPage,
        sortBy: currentFilters.sortBy,
      };

      const filterOptions: FilterOptions = {
        searchQuery: currentFilters.searchQuery,
        familyId: currentFilters.familyId,
        type: currentFilters.type,
        memberId: currentFilters.memberId,
        startDate: currentFilters.startDate,
        endDate: currentFilters.endDate,
        calendarType: currentFilters.calendarType,
        lunarMonthRange: currentFilters.lunarMonthRange, // Updated to new property
      };

      const response = await eventService.search(listOptions, filterOptions); // Use injected service
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    placeholderData: (previousData: Paginated<Event> | undefined) => previousData, // Keep previous data while fetching new
    staleTime: 1000 * 60 * 1, // 1 minute
  });

  const events = computed(() => query.data.value?.items || []);
  const totalItems = computed(() => query.data.value?.totalItems || 0);
  const loading = computed(() => query.isFetching.value);

  return {
    query,
    events,
    totalItems,
    loading,
    error: query.error,
    refetch: query.refetch,
  };
}

export type UseEventsQueryReturn = ReturnType<typeof useEventsQuery>;