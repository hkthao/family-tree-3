import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';
import type { Paginated, UserDto } from '@/types';

// Instantiate the service outside the composable to avoid re-instantiation on every call
const userService = new ApiUserService(apiClient);

interface UseUserSearchOptions {
  debounceTime?: number;
}

export function useUserSearch(options?: UseUserSearchOptions) {
  const { debounceTime = 300 } = options || {};

  const searchTerm = ref('');
  const debouncedSearchTerm = ref('');
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  const { data, isLoading, isFetching, error } = useQuery<Paginated<UserDto>, Error>({
    queryKey: ['users', debouncedSearchTerm],
    queryFn: async () => {
      if (!debouncedSearchTerm.value) {
        return { items: [] as UserDto[], page: 1, totalItems: 0, totalPages: 0 };
      }
      const result = await userService.search(debouncedSearchTerm.value, 1, 10);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    staleTime: 1000 * 60 * 1, // 1 minute
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
    users: computed(() => data.value?.items || []),
    isLoading: computed(() => isLoading.value || isFetching.value),
    error,
  };
}
