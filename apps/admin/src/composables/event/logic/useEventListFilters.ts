import { ref, computed } from 'vue';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { EventFilter } from '@/types'; // Use EventFilter

export function useEventListFilters() {
  const filters = ref<EventFilter>({ // Use EventFilter
    searchQuery: '',
    page: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    sortBy: [],
  });

  const setPage = (newPage: number) => {
    filters.value.page = newPage;
  };

  const setItemsPerPage = (newItemsPerPage: number) => {
    if (filters.value.itemsPerPage != newItemsPerPage) {
      filters.value.itemsPerPage = newItemsPerPage;
      filters.value.page = 1; // Reset to first page when items per page changes
    }
  };

  const setSortBy = (newSortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    filters.value.sortBy = newSortBy;
  };

  const setSearchQuery = (newSearchQuery: string) => {
    filters.value.searchQuery = newSearchQuery;
    filters.value.page = 1; // Reset to first page on new search
  };

  const setFilters = (newFilters: EventFilter) => {
    if (newFilters.searchQuery !== undefined) {
      setSearchQuery(newFilters.searchQuery);
    }
    if (newFilters.page !== undefined) {
      setPage(newFilters.page);
    }
    if (newFilters.itemsPerPage !== undefined) {
      setItemsPerPage(newFilters.itemsPerPage);
    }
    if (newFilters.sortBy !== undefined) {
      setSortBy(newFilters.sortBy);
    }
    // Handle other filters directly as they don't have side effects that reset page
    if (newFilters.familyId !== undefined) {
      filters.value.familyId = newFilters.familyId;
    }
    if (newFilters.startDate !== undefined) {
      filters.value.startDate = newFilters.startDate;
    }
    if (newFilters.endDate !== undefined) {
      filters.value.endDate = newFilters.endDate;
    }
    if (newFilters.calendarType !== undefined) {
      filters.value.calendarType = newFilters.calendarType;
    }
  };

  return {
    searchQuery: computed(() => filters.value.searchQuery),
    page: computed(() => filters.value.page),
    itemsPerPage: computed(() => filters.value.itemsPerPage),
    sortBy: computed(() => filters.value.sortBy),
    filters,
    setPage,
    setItemsPerPage,
    setSortBy,
    setSearchQuery,
    setFilters,
  };
}

export type UseEventListFiltersReturn = ReturnType<typeof useEventListFilters>;