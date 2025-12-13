import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Family, FamilyFilter, Paginated, ListOptions, FilterOptions } from '@/types';
import { ApiFamilyService } from '@/services/family/api.family.service';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyService: IFamilyService = new ApiFamilyService(apiClient);

export function useFamiliesQuery(filters: Ref<FamilyFilter>) {
  const query = useQuery<Paginated<Family>, Error>({
    queryKey: computed(() => queryKeys.families.list(unref(filters))),
    queryFn: async () => {
      const currentFilters = unref(filters);

      const listOptions: ListOptions = {
        page: currentFilters.page,
        itemsPerPage: currentFilters.itemsPerPage,
        sortBy: currentFilters.sortBy,
      };

      const filterOptions: FilterOptions = {
        searchQuery: currentFilters.searchQuery,
        visibility: currentFilters.visibility,
        // Add other specific family filters here if they exist in FamilyFilter
      };

      const response = await apiFamilyService.search(listOptions, filterOptions);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    placeholderData: (previousData: Paginated<Family> | undefined) => previousData, // Keep previous data while fetching new
    staleTime: 1000 * 60 * 1, // 1 minute
  });

  const families = computed(() => query.data.value?.items || []);
  const totalItems = computed(() => query.data.value?.totalItems || 0);
  const loading = computed(() => query.isFetching.value);

  return {
    query,
    families,
    totalItems,
    loading,
    error: query.error,
    refetch: query.refetch,
  };
}