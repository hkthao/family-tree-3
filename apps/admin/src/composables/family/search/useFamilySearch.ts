import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyDto } from '@/types';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { useServices } from '@/plugins/services.plugin';

interface UseFamilySearchOptions {
  debounceTime?: number;
  service?: IFamilyService;
}

export function useFamilySearch(options?: UseFamilySearchOptions) {
  const { debounceTime = 300, service = useServices().family } = options || {};

  const searchQuery = ref('');
  const debouncedSearchQuery = ref('');
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  const { data, isLoading, isFetching, error } = useQuery<FamilyDto[], Error>({
    queryKey: ['families', debouncedSearchQuery],
    queryFn: async () => {
      if (!debouncedSearchQuery.value) {
        return [];
      }
      const result = await service.search({ page: 1, itemsPerPage: 10 }, { name: debouncedSearchQuery.value });
      if (result.ok) {
        return result.value.items;
      }
      throw result.error;
    },
    // Keep data fresh for a short period, but not too long for search results
    staleTime: 1000 * 60 * 1, // 1 minute
    // Only fetch if debouncedSearchQuery has a value
    enabled: computed(() => !!debouncedSearchQuery.value),
  });

  watch(searchQuery, (newSearchQuery) => {
    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }
    debounceTimer = setTimeout(() => {
      debouncedSearchQuery.value = newSearchQuery;
    }, debounceTime);
  });

  return {
    state: {
      searchQuery,
      families: data,
      isLoading: computed(() => isLoading.value || isFetching.value),
      error,
    },
    actions: {}, // No explicit actions defined within this composable beyond updating searchQuery
  };
}
