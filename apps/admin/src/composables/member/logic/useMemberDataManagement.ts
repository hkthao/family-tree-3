import { ref, watch, computed } from 'vue';
import type { MemberFilter, ListOptions } from '@/types';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { useDebounceFn } from '@vueuse/core';

export const useMemberDataManagement = (initialFamilyId?: string) => {
  const searchQuery = ref('');
  const currentPage = ref(1);
  const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);
  const sortBy = ref<{ key: string; order: 'asc' | 'desc' }[]>([]);
  const filters = ref<MemberFilter>({
    familyId: initialFamilyId,
    searchQuery: '',
  });

  const paginationOptions = computed<ListOptions>(() => ({
    page: currentPage.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value,
  }));

  const setPage = (page: number) => {
    currentPage.value = page;
  };

  const setItemsPerPage = (perPage: number) => {
    itemsPerPage.value = perPage;
    currentPage.value = 1; // Reset to first page when items per page changes
  };

  const setSortBy = (newSortBy: { key: string; order: 'asc' | 'desc' }[]) => {
    sortBy.value = newSortBy;
  };

  const setSearchQuery = (search: string) => {
    searchQuery.value = search;
    debouncedUpdateFilters();
  };

  const setFilters = (newFilters: MemberFilter) => {
    filters.value = { ...filters.value, ...newFilters };
    currentPage.value = 1; // Reset to first page when filters change
  };

  const debouncedUpdateFilters = useDebounceFn(() => {
    filters.value = { ...filters.value, searchQuery: searchQuery.value };
    currentPage.value = 1;
  }, 300);

  // Watch for changes in familyId prop
  watch(() => initialFamilyId, (newFamilyId) => {
    filters.value.familyId = newFamilyId;
    currentPage.value = 1;
  });

  // Watch for changes in searchQuery and filters to trigger an update (if not debounced)
  watch([filters], () => {
    // This watch is mainly to react to external filter changes that don't go through setFilters
    // or to ensure filters are updated after debounce for searchQuery
  }, { deep: true });

  return {
    searchQuery,
    currentPage,
    itemsPerPage,
    sortBy,
    filters,
    paginationOptions,
    setPage,
    setItemsPerPage,
    setSortBy,
    setSearchQuery,
    setFilters,
  };
};
