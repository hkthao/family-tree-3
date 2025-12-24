import { ref, computed, watch } from 'vue';
import { formatDate } from '@/utils/dateUtils';
import { useI18n } from 'vue-i18n';
import type { EventDto, ListOptions, EventFilter, Paginated } from '@/types';
import { useQuery, type UseQueryReturnType } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { type EventServiceAdapter, DefaultEventServiceAdapter } from '../event.adapter';
import { mapFiltersToQueryOptions, sortEventsBySolarDateDesc } from './eventTimeline.logic';

interface UseEventTimelineDeps {
  useI18n: typeof useI18n;
  eventService: EventServiceAdapter;
  useQuery: <TData = unknown, TError = Error>(
    options: any,
  ) => UseQueryReturnType<TData, TError>;
  formatDate: (date: Date | string | null | undefined) => string;
}

const defaultDeps: UseEventTimelineDeps = {
  useI18n,
  eventService: DefaultEventServiceAdapter,
  useQuery,
  formatDate,
};

export function useEventTimeline(
  props: { familyId?: string; memberId?: string; readOnly?: boolean },
  deps: UseEventTimelineDeps = defaultDeps,
) {
  const { t } = deps.useI18n();
  const { eventService } = deps;

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
  const { data, isLoading, error, refetch } = deps.useQuery<Paginated<EventDto>, Error>({
    queryKey: computed(() => [queryKeys.events.list(filters.value), currentPage.value, itemsPerPage.value, sortBy.value]),
    queryFn: async () => {
      const { listOptions, filterOptions } = mapFiltersToQueryOptions(
        filters.value,
        currentPage.value,
        itemsPerPage.value,
        sortBy.value,
      );
      const result = await eventService.search(listOptions, filterOptions);
      if (result.ok) {
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!(filters.value.familyId || filters.value.memberId)),
    staleTime: 5 * 60 * 1000, // 5 minutes
    placeholderData: { items: [], totalPages: 0, totalCount: 0 },
    select: sortEventsBySolarDateDesc, // Use extracted logic
  });

  const list = computed(() => ({
    items: data.value?.items || [],
    loading: isLoading.value,
    error: error.value?.message || null,
    currentPage: currentPage.value,
    itemsPerPage: itemsPerPage.value,
    totalPages: data.value?.totalPages || 0,
    totalCount: data.value?.totalItems || 0,
    sortBy: sortBy.value,
  }));

  const paginationLength = computed(() => {
    return Math.max(1, list.value.totalPages);
  });

  const showEventDetails = (event: EventDto) => {
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

  const handlePropsChange = ([newFamilyId, newMemberId]: [string | undefined, string | undefined]) => {
    setFilters({ familyId: newFamilyId, memberId: newMemberId });
  };

  watch(
    [() => props.familyId, () => props.memberId],
    handlePropsChange,
    { immediate: true },
  );

  return {
    state: {
      list,
      selectedEventId,
      detailDrawer,
      paginationLength,
      isLoading,
    },
    actions: {
      t,
      showEventDetails,
      handleDetailClosed,
      handlePageChange,
      formatDate: deps.formatDate,
      setListOptions,
      setFilters,
    },
  };
}
