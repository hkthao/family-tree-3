import { reactive, watch, unref } from 'vue';
import type { Ref } from 'vue';
import type { FamilyLocationFilter, ListOptions, LocationType, LocationSource } from '@/types';

export interface FamilyLocationSearchCriteria {
  locationType?: LocationType;
  locationSource?: LocationSource;
}

export const useFamilyLocationDataManagement = (familyId: Ref<string | undefined> | string) => {
  const filters = reactive<FamilyLocationFilter>({
    familyId: unref(familyId),
  });
  const paginationOptions = reactive<ListOptions>({
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
    filters,
    paginationOptions,
    setFilters,
    setPage,
    setItemsPerPage,
    setSortBy,
  };
};
