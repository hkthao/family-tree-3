import { ref, computed, watch, unref, type Ref } from 'vue';
import { useFamilyLocationsQuery } from './queries/useFamilyLocationsQuery';
import type { FamilyLocation, FamilyLocationFilter, ListOptions } from '@/types';
import { useDebouncedSearch } from '@/composables/family/logic/useDebouncedSearch';

export function useFamilyLocationSearch(familyIdRef: Ref<string | null>) {
  const { state: debouncedSearchState } = useDebouncedSearch('', 300); // Use useDebouncedSearch
  const page = ref(1);
  const itemsPerPage = ref(10);
  const sortBy = ref<ListOptions['sortBy']>([]);

  const filterOptions = computed<FamilyLocationFilter>(() => ({
    familyId: unref(familyIdRef) || undefined, // Use unref and handle null/empty string
    searchQuery: debouncedSearchState.debouncedSearchQuery.value, // Use debounced value
  }));

  const paginationOptions = computed<ListOptions>(() => ({
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value,
  }));

  const {
    data: paginatedFamilyLocations,
    isLoading,
    isFetching,
    refetch,
  } = useFamilyLocationsQuery(paginationOptions, filterOptions);

  const familyLocations = computed<FamilyLocation[]>(
    () => paginatedFamilyLocations.value?.items || [],
  );
  const totalItems = computed(() => paginatedFamilyLocations.value?.totalItems || 0);

  // Watch for changes in familyIdRef to reset pagination and refetch
  watch(
    familyIdRef,
    () => {
      page.value = 1;
      sortBy.value = [];
      refetch();
    },
  );

  return {
    familyLocations,
    totalItems,
    isLoading: computed(() => isLoading.value || isFetching.value),
    searchQuery: debouncedSearchState.searchQuery, // Expose the non-debounced search query
    page,
    itemsPerPage,
    sortBy,
    refetch,
  };
}
