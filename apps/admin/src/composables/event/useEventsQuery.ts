import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Event, EventFilter, Paginated, ListOptions, FilterOptions } from '@/types';
import { ApiEventService } from '@/services/event/api.event.service'; // This will be created soon
import type { IEventService } from '@/services/event/event.service.interface'; // This will be created soon
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiEventService: IEventService = new ApiEventService(apiClient);

export function useEventsQuery(filters: Ref<EventFilter>) {
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
        // Removed old date and location filters
        // startDate: currentFilters.startDate,
        // endDate: currentFilters.endDate,
        // location: currentFilters.location,
        memberId: currentFilters.memberId,
        // New filters for backend
        minSolarDate: currentFilters.minSolarDate,
        maxSolarDate: currentFilters.maxSolarDate,
        calendarType: currentFilters.calendarType,
      };

      const response = await apiEventService.search(listOptions, filterOptions);
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