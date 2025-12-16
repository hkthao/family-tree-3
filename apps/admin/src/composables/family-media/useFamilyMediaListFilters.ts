import { ref, computed, unref, type Ref, watch } from 'vue';
import type { FamilyMediaFilter, ListOptions } from '@/types';

export function useFamilyMediaListFilters(familyId: Ref<string | undefined>) {
  const defaultItemsPerPage = 10;

  const page = ref(1);
  const itemsPerPage = ref(defaultItemsPerPage);
  const sortBy = ref<ListOptions['sortBy']>([]);
  const filters = ref<FamilyMediaFilter>({
    mediaType: undefined,
  });

  // Watch for changes in the passed familyId and update the internal filters accordingly
  watch(familyId, (newFamilyId) => {
    filters.value.familyId = newFamilyId;
    page.value = 1; // Reset to first page when familyId changes
  }, { immediate: true }); // immediate: true to run once on setup

  const listOptions = computed<ListOptions>(() => ({
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value,
  }));

  const setPage = (newPage: number) => {
    page.value = newPage;
  };

  const setItemsPerPage = (newItemsPerPage: number) => {
    itemsPerPage.value = newItemsPerPage;
    page.value = 1; // Reset to first page when items per page changes
  };

  const setSortBy = (newSortBy: ListOptions['sortBy']) => {
    sortBy.value = newSortBy;
  };

  const setFilters = (newFilters: FamilyMediaFilter) => {
    filters.value = {
      ...newFilters,
      familyId: unref(familyId), // Always ensure familyId from prop is used
    };
    page.value = 1; // Reset to first page when filters change
  };

  return {
    page,
    itemsPerPage,
    sortBy,
    filters,
    listOptions,
    setPage,
    setItemsPerPage,
    setSortBy,
    setFilters,
  };
}