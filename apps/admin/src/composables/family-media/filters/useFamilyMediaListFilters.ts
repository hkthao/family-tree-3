import { ref, type Ref, watch } from 'vue';
import type { FamilyMediaFilter, ListOptions } from '@/types';

interface UseFamilyMediaListFiltersOptions {
  familyId: Ref<string | undefined>;
  defaultItemsPerPage?: number;
}

export function useFamilyMediaListFilters(options: UseFamilyMediaListFiltersOptions) {
  const { familyId, defaultItemsPerPage = 10 } = options;

  const page = ref(1);
  const itemsPerPage = ref(defaultItemsPerPage);
  const sortBy = ref<ListOptions['sortBy']>([]);
  const filters = ref<FamilyMediaFilter>({
    searchQuery: undefined,
    mediaType: undefined,
  });

  // Watch for changes in the passed familyId and update the internal filters accordingly
  watch(familyId, (newFamilyId) => {
    filters.value.familyId = newFamilyId;
    page.value = 1; // Reset to first page when familyId changes
  }, { immediate: true }); // immediate: true to run once on setup

  // Removed listOptions computed property

  const setPage = (newPage: number) => {
    page.value = newPage;
  };

  const setItemsPerPage = (newItemsPerPage: number) => {
    itemsPerPage.value = newItemsPerPage;
  };

  const setSortBy = (newSortBy: ListOptions['sortBy']) => {
    sortBy.value = newSortBy;
  };

  const setFilters = (newFilters: FamilyMediaFilter) => {
    filters.value = { ...filters.value, ...newFilters }; // Update the Ref value
  };

  return {
    state: {
      page,
      itemsPerPage,
      sortBy,
      filters,
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
      setFilters,
    },
  };
}