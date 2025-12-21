import { ref, watch } from 'vue';
import type { Ref } from 'vue';
import type { FamilyDictFilter } from '@/types';

export function useFamilyDictListFilters() {
  const page = ref(1);
  const itemsPerPage = ref(10);
  const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([]);
  const searchQuery = ref('');
  const filters = ref<FamilyDictFilter>({}); // Changed to Ref and initialized with an empty object

  const setPage = (newPage: number) => {
    page.value = newPage;
  };

  const setItemsPerPage = (newItemsPerPage: number) => {
    itemsPerPage.value = newItemsPerPage;
  };

  const setSortBy = (newSortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    sortBy.value = newSortBy;
  };

  const setSearchQuery = (newSearchQuery: string) => {
    searchQuery.value = newSearchQuery;
  };

  const setFilters = (newFilters: FamilyDictFilter) => {
    filters.value = { ...filters.value, ...newFilters }; // Update the Ref value
  };

  watch([searchQuery, page, itemsPerPage, sortBy], () => {
    filters.value = {
      searchQuery: searchQuery.value,
      page: page.value,
      itemsPerPage: itemsPerPage.value,
      sortBy: sortBy.value,
    };
  }, { deep: true, immediate: true });


  return {
    state: {
      page,
      itemsPerPage,
      sortBy,
      searchQuery,
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