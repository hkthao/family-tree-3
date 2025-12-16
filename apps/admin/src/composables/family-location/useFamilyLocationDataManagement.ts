import { ref, reactive, watch, unref } from 'vue';
import type { Ref } from 'vue';
import type { FamilyLocationFilter, ListOptions } from '@/types'; // Import ListOptions
import { removeDiacritics } from '@/utils/string.utils';
import { useDebounceFn } from '@vueuse/core';

export const useFamilyLocationDataManagement = (familyId: Ref<string | undefined> | string) => {
  const searchQuery = ref('');
  const filters = reactive<FamilyLocationFilter>({
    familyId: unref(familyId),
    searchQuery: '',
  });
  const paginationOptions = reactive<ListOptions>({ // Changed type to ListOptions
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
  });

  // Watch for changes in familyId prop
  watch(
    () => unref(familyId),
    (newFamilyId) => {
      filters.familyId = newFamilyId;
    },
    { immediate: true },
  );

  const debouncedSetSearchQuery = useDebounceFn((value: string) => {
    filters.searchQuery = value;
    paginationOptions.page = 1; // Reset to first page on new search
  }, 300);

  const setSearchQuery = (query: string) => {
    searchQuery.value = query;
    debouncedSetSearchQuery(removeDiacritics(query));
  };

  const setFilters = (newFilters: FamilyLocationFilter) => {
    Object.assign(filters, newFilters);
    paginationOptions.page = 1; // Reset to first page on new filters
  };

  const setPage = (page: number) => {
    paginationOptions.page = page;
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    paginationOptions.itemsPerPage = itemsPerPage;
  };

  const setSortBy = (sortBy: Array<{ key: string; order: 'asc' | 'desc' }>) => {
    paginationOptions.sortBy = sortBy;
  };

  return {
    searchQuery,
    filters,
    paginationOptions,
    setSearchQuery,
    setFilters,
    setPage,
    setItemsPerPage,
    setSortBy,
  };
};
