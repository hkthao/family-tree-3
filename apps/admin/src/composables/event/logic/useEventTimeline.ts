import { ref, computed, watch } from 'vue';
import { formatDate } from '@/utils/dateUtils';
import { useI18n } from 'vue-i18n';
import type { Event, ListOptions, EventFilter } from '@/types';
import { useServices } from '@/composables';
import { useQuery } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';

export function useEventTimeline(props: { familyId?: string; memberId?: string; readOnly?: boolean }) {
  const { t } = useI18n();
  const { event: eventService } = useServices(); // Renamed to avoid conflict
  

  const selectedEventId = ref<string | null>(null);
  const detailDrawer = ref(false);

  // Pagination and filter state
  const currentPage = ref(1);
  const itemsPerPage = ref(10); // Default items per page
  const sortBy = ref<ListOptions['sortBy']>([]);
  const filters = ref<EventFilter>({
    familyId: props.familyId,
    memberId: props.memberId,
  });

  // Fetch events using useQuery
  const { data, isLoading, error, refetch } = useQuery<{ items: Event[]; totalPages: number; totalCount: number }, Error>({
    queryKey: [queryKeys.events.list(filters.value), currentPage.value, itemsPerPage.value, sortBy.value],
    queryFn: async () => {
      const currentFilters = {
        ...filters.value,
        page: currentPage.value,
        itemsPerPage: itemsPerPage.value,
        sortBy: sortBy.value,
      };
      const result = await eventService.search(currentFilters, currentFilters); // Pass currentFilters as both options and filters
      if (result.ok) {
        return {
          items: result.value.items,
          totalPages: result.value.totalPages,
          totalCount: result.value.totalItems,
        };
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!(filters.value.familyId || filters.value.memberId)),
    staleTime: 5 * 60 * 1000, // 5 minutes
    placeholderData: { items: [], totalPages: 0, totalCount: 0 },
    select: (data) => {
      // Sort by solarDate in descending order
      const sortedItems = [...data.items].sort((a, b) => {
        if (!a.solarDate || !b.solarDate) return 0;
        return new Date(b.solarDate).getTime() - new Date(a.solarDate).getTime();
      });
      return { ...data, items: sortedItems };
    },
  });

  const list = computed(() => ({
    items: data.value?.items || [],
    loading: isLoading.value,
    error: error.value?.message || null,
    currentPage: currentPage.value,
    itemsPerPage: itemsPerPage.value,
    totalPages: data.value?.totalPages || 0,
    totalCount: data.value?.totalCount || 0,
    sortBy: sortBy.value,
  }));

  const paginationLength = computed(() => {
    return Math.max(1, list.value.totalPages);
  });

  const showEventDetails = (event: Event) => {
    selectedEventId.value = event.id;
    detailDrawer.value = true;
  };

  const handleDetailClosed = () => {
    detailDrawer.value = false;
    selectedEventId.value = null;
  };

  const setListOptions = (options: { page?: number; itemsPerPage?: number; sortBy?: ListOptions['sortBy'] }) => {
    if (options.page !== undefined) currentPage.value = options.page;
    if (options.itemsPerPage !== undefined) itemsPerPage.value = options.itemsPerPage;
    if (options.sortBy !== undefined) sortBy.value = options.sortBy;
  };

  const setFilters = (newFilters: EventFilter) => {
    filters.value = { ...filters.value, ...newFilters };
    currentPage.value = 1; // Reset page on filter change
    refetch(); // Trigger refetch with new filters
  };

  const handlePageChange = (newPage: number) => {
    setListOptions({ page: newPage });
  };

  watch(
    [() => props.familyId, () => props.memberId],
    ([newFamilyId, newMemberId]) => {
      setFilters({ familyId: newFamilyId, memberId: newMemberId });
    },
    { immediate: true },
  );

  return {
    t,
    list,
    selectedEventId,
    detailDrawer,
    paginationLength,
    showEventDetails,
    handleDetailClosed,
    handlePageChange,
    formatDate,
    setListOptions,
    setFilters,
    isLoading, // Expose isLoading directly
  };
}
