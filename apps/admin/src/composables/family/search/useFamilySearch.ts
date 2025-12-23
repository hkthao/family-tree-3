import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Family } from '@/types';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { useServices } from '@/plugins/services.plugin';

interface UseFamilySearchOptions {
  debounceTime?: number;
  service?: IFamilyService;
}

export function useFamilySearch(options?: UseFamilySearchOptions) {
  const { debounceTime = 300, service = useServices().family } = options || {};

  const searchTerm = ref('');
  const debouncedSearchTerm = ref('');
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  const { data, isLoading, isFetching, error } = useQuery<Family[], Error>({
    queryKey: ['families', debouncedSearchTerm],
    queryFn: async () => {
      if (!debouncedSearchTerm.value) {
        return [];
      }
      const result = await service.search({ page: 1, itemsPerPage: 10 }, { name: debouncedSearchTerm.value });
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
    state: {
      searchTerm,
      families: data,
      isLoading: computed(() => isLoading.value || isFetching.value),
      error,
    },
    actions: {}, // No explicit actions defined within this composable beyond updating searchTerm
  };
}
