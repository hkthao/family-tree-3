import { ref, computed } from 'vue';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyFilter } from '@/types';

export function useFamilyListFilters() {
  const filters = ref<FamilyFilter>({
    searchQuery: '',
    page: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    sortBy: [],
  });

  const setPage = (newPage: number) => {
    filters.value.page = newPage;
  };

  const setItemsPerPage = (newItemsPerPage: number) => {
    if (filters.value.itemsPerPage !== newItemsPerPage) {
      filters.value.itemsPerPage = newItemsPerPage;
      filters.value.page = 1; // Reset to first page when items per page changes
    }
  };

  const setSortBy = (newSortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    filters.value.sortBy = newSortBy;
  };

  const setSearchQuery = (newSearchQuery: string) => {
    if (filters.value.searchQuery !== newSearchQuery) {
      filters.value.searchQuery = newSearchQuery;
      filters.value.page = 1; // Reset to first page on new search
    }
  };

  const setFilters = (newFilters: FamilyFilter) => {
    const oldFilters = { ...filters.value };
    filters.value = { ...oldFilters, ...newFilters };

    // If any filter other than 'page' has changed, and newFilters.page was not explicitly set, reset to page 1.
    const hasNonPageFilterChanged = Object.keys(newFilters).some(key =>
      key !== 'page' && oldFilters[key] !== newFilters[key]
    );

    if (hasNonPageFilterChanged && newFilters.page === undefined) {
      filters.value.page = 1;
    }
  };

  return {
    state: {
      searchQuery: computed(() => filters.value.searchQuery),
      page: computed(() => filters.value.page),
      itemsPerPage: computed(() => filters.value.itemsPerPage),
      sortBy: computed(() => filters.value.sortBy),
      filters,
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
      setSearchQuery,
      setFilters,
    },
  };
}