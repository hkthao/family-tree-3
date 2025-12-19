import { ref, watch, unref, type Ref } from 'vue';
import type { FamilyLocationFilter, ListOptions, LocationType, LocationSource } from '@/types';

export interface FamilyLocationSearchCriteria {
  locationType?: LocationType;
  locationSource?: LocationSource;
}

export const useFamilyLocationDataManagement = (familyId: Ref<string | undefined> | string) => {
  const filters = ref<FamilyLocationFilter>({
    familyId: unref(familyId),
  });
  const paginationOptions = ref<ListOptions>({
    page: 1,
    itemsPerPage: 10,
    sortBy: [],
  });

  // Watch for changes in familyId prop
  watch(
    () => unref(familyId),
    (newFamilyId) => {
      filters.value.familyId = newFamilyId;
    },
    { immediate: true },
  );

  const setFilters = (newFilters: FamilyLocationFilter) => {
    Object.assign(filters.value, newFilters);
    paginationOptions.value.page = 1; // Reset to first page on new filters
  };

  const setPage = (page: number) => {
    paginationOptions.value.page = page;
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    paginationOptions.value.itemsPerPage = itemsPerPage;
  };

  const setSortBy = (sortBy: Array<{ key: string; order: 'asc' | 'desc' }>) => {
    paginationOptions.value.sortBy = sortBy;
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
