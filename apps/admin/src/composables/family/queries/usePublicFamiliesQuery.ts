import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyDto, FamilyFilter, Paginated, ListOptions, FilterOptions } from '@/types';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';

export function usePublicFamiliesQuery(
  filters: Ref<FamilyFilter>,
  service: IFamilyService = useServices().family,
) {
  const query = useQuery<Paginated<FamilyDto>, Error>({
    queryKey: computed(() => queryKeys.publicFamilies.list(unref(filters))), // Use a new queryKey for public families
    queryFn: async () => {
      const currentFilters = unref(filters);

      const listOptions: ListOptions = {
        page: currentFilters.page,
        itemsPerPage: currentFilters.itemsPerPage,
        sortBy: currentFilters.sortBy,
      };

      const filterOptions: FilterOptions = {
        searchQuery: currentFilters.searchQuery,
        // Removed visibility filter as searchPublic implicitly handles public families
      };

      const response = await service.searchPublic(listOptions, filterOptions); // Call searchPublic
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    placeholderData: (previousData: Paginated<FamilyDto> | undefined) => previousData, // Keep previous data while fetching new
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