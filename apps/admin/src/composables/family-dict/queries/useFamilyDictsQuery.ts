import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyDict, FamilyDictFilter, Paginated, ListOptions, FilterOptions } from '@/types';
import { ApiFamilyDictService } from '@/services/family-dict/api.family-dict.service';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyDictService: IFamilyDictService = new ApiFamilyDictService(apiClient);

export function useFamilyDictsQuery(filters: Ref<FamilyDictFilter>) {
  const query = useQuery<Paginated<FamilyDict>, Error>({
    queryKey: computed(() => queryKeys.familyDicts.list(unref(filters))),
    queryFn: async () => {
      const currentFilters = unref(filters);

      const listOptions: ListOptions = {
        page: currentFilters.page,
        itemsPerPage: currentFilters.itemsPerPage,
        sortBy: currentFilters.sortBy,
      };

      const filterOptions: FilterOptions = {
        searchQuery: currentFilters.searchQuery,
        lineage: currentFilters.lineage,
        region: currentFilters.region,
      };

      const response = await apiFamilyDictService.search(listOptions, filterOptions);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    placeholderData: (previousData: Paginated<FamilyDict> | undefined) => previousData,
    staleTime: 1000 * 60 * 1, // 1 minute
  });

  const familyDicts = computed(() => query.data.value?.items || []);
  const totalItems = computed(() => query.data.value?.totalItems || 0);
  const loading = computed(() => query.isFetching.value);

  return {
    query,
    familyDicts,
    totalItems,
    loading,
    error: query.error,
    refetch: query.refetch,
  };
}