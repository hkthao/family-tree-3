import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { ApiFamilyService } from '@/services/family/api.family.service';
import apiClient from '@/plugins/axios';
import type { Family } from '@/types';

// Instantiate the service outside the composable to avoid re-instantiation on every call
const familyService = new ApiFamilyService(apiClient);

interface UseFamilySearchOptions {
  debounceTime?: number;
}

export function useFamilySearch(options?: UseFamilySearchOptions) {
  const { debounceTime = 300 } = options || {};

  const searchTerm = ref('');
  const debouncedSearchTerm = ref('');
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  const { data, isLoading, isFetching, error } = useQuery<Family[], Error>({
    queryKey: ['families', debouncedSearchTerm],
    queryFn: async () => {
      if (!debouncedSearchTerm.value) {
        return [];
      }
      const result = await familyService.search({ page: 1, itemsPerPage: 10 }, { name: debouncedSearchTerm.value });
      if (result.ok) {
        return result.value.items;
      }
      throw result.error;
    },
    // Keep data fresh for a short period, but not too long for search results
    staleTime: 1000 * 60 * 1, // 1 minute
    // Only fetch if debouncedSearchTerm has a value
    enabled: computed(() => !!debouncedSearchTerm.value),
  });

  watch(searchTerm, (newSearchTerm) => {
    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }
    debounceTimer = setTimeout(() => {
      debouncedSearchTerm.value = newSearchTerm;
    }, debounceTime);
  });

  return {
    searchTerm,
    families: data,
    isLoading: computed(() => isLoading.value || isFetching.value),
    error,
  };
}
