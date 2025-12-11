import { ref, watch } from 'vue';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyFilter } from '@/types';

export function useFamilyListFilters() {
  const searchQuery = ref('');
  const page = ref(1);
  const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
  const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([]);
  const filters = ref<FamilyFilter>({});

  // Watch for changes in individual filter/pagination/sort states and update the combined filters object
  watch([searchQuery, page, itemsPerPage, sortBy], () => {
    filters.value = {
      searchQuery: searchQuery.value,
      page: page.value,
      itemsPerPage: itemsPerPage.value,
      sortBy: sortBy.value,
      // Add other filter criteria here as needed
    };
  }, { deep: true, immediate: true });

  const setPage = (newPage: number) => {
    page.value = newPage;
  };

  const setItemsPerPage = (newItemsPerPage: number) => {
    itemsPerPage.value = newItemsPerPage;
    page.value = 1; // Reset to first page when items per page changes
  };

  const setSortBy = (newSortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    sortBy.value = newSortBy;
  };

  const setSearchQuery = (newSearchQuery: string) => {
    searchQuery.value = newSearchQuery;
    page.value = 1; // Reset to first page on new search
  };

  const setFilters = (newFilters: FamilyFilter) => {
    searchQuery.value = newFilters.searchQuery || '';
    page.value = newFilters.page || 1;
    itemsPerPage.value = newFilters.itemsPerPage || DEFAULT_ITEMS_PER_PAGE;
    sortBy.value = newFilters.sortBy || [];
    // Update other filter criteria
    Object.assign(filters.value, newFilters);
  };

  return {
    searchQuery,
    page,
    itemsPerPage,
    sortBy,
    filters,
    setPage,
    setItemsPerPage,
    setSortBy,
    setSearchQuery,
    setFilters,
  };
}