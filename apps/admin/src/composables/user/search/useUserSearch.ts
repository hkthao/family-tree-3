import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Paginated, UserDto } from '@/types';
import type { IUserService } from '@/services/user/user.service.interface';
import { useServices } from '@/composables';

interface UseUserSearchOptions {
  debounceTime?: number;
  userService?: IUserService;
}

export function useUserSearch(options: UseUserSearchOptions = {}) {
  const { debounceTime = 300, userService = useServices().user } = options;

  const searchTerm = ref('');
  const debouncedSearchTerm = ref('');
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  const { data, isLoading, isFetching, error } = useQuery<Paginated<UserDto>, Error>({
    queryKey: ['users', debouncedSearchTerm],
    queryFn: (async () => {
      if (!debouncedSearchTerm.value) {
        return { items: [] as UserDto[], page: 1, totalItems: 0, totalPages: 0 } as Paginated<UserDto>;
      }
      const result = await userService.search(debouncedSearchTerm.value, 1, 10);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    }) as () => Promise<Paginated<UserDto>>,
    staleTime: 1000 * 60 * 1, // 1 minute
    enabled: computed(() => !!debouncedSearchTerm.value),
  });

  const handleSearchTermChange = (newSearchTerm: string) => {
    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }
    debounceTimer = setTimeout(() => {
      debouncedSearchTerm.value = newSearchTerm;
    }, debounceTime);
  };

  watch(searchTerm, handleSearchTermChange);

  return {
    state: {
      searchTerm,
      users: computed(() => (data.value as Paginated<UserDto> | undefined)?.items || []),
      isLoading: computed(() => isLoading.value || isFetching.value),
      error,
    },
  };
}
