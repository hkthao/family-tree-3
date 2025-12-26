import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Paginated, UserDto } from '@/types';
import type { IUserService } from '@/services/user/user.service.interface';
import { useServices } from '@/plugins/services.plugin';

interface UseUserSearchOptions {
  debounceTime?: number;
  userService?: IUserService;
}

export function useUserSearch(options: UseUserSearchOptions = {}) {
  const { debounceTime = 300, userService = useServices().user } = options;

  const searchQuery = ref('');
  const debouncedSearchQuery = ref('');
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  const { data, isLoading, isFetching, error } = useQuery<Paginated<UserDto>, Error>({
    queryKey: ['users', debouncedSearchQuery],
    queryFn: (async () => {
      if (!debouncedSearchQuery.value) {
        return { items: [] as UserDto[], page: 1, totalItems: 0, totalPages: 0 } as Paginated<UserDto>;
      }
      const result = await userService.search(debouncedSearchQuery.value, 1, 10);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    }) as () => Promise<Paginated<UserDto>>,
    staleTime: 1000 * 60 * 1, // 1 minute
    enabled: computed(() => !!debouncedSearchQuery.value),
  });

  const handleSearchQueryChange = (newSearchQuery: string) => {
    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }
    debounceTimer = setTimeout(() => {
      debouncedSearchQuery.value = newSearchQuery;
    }, debounceTime);
  };

  watch(searchQuery, handleSearchQueryChange);

  return {
    state: {
      searchQuery,
      users: computed(() => (data.value as Paginated<UserDto> | undefined)?.items || []),
      isLoading: computed(() => isLoading.value || isFetching.value),
      error,
    },
  };
}
