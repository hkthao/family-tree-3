import { ref, watch, computed, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { UserDto, Paginated } from '@/types';
import { queryKeys as QueryKeys } from '@/constants/queryKeys';
import type { IUserService } from '@/services/user/user.service.interface';
import { useServices } from '@/composables';

interface UseUserAutocompleteSearchOptions {
  userService?: () => IUserService;
}

const defaultOptions: UseUserAutocompleteSearchOptions = {
  userService: () => useServices().user,
};

export function useUserAutocompleteSearch(searchQuery: Readonly<Ref<string>>, options: UseUserAutocompleteSearchOptions = defaultOptions) {
  const { userService: getUserService = defaultOptions.userService! } = options;
  const users = ref<UserDto[]>([]);

  const userService = getUserService();

  const { isLoading, isFetching, error, data } = useQuery<Paginated<UserDto>, Error>({
    queryKey: QueryKeys.users.search(searchQuery.value),
    queryFn: (async () => {
      if (!searchQuery.value) {
        return {
          items: [],
          page: 1,
          totalItems: 0,
          totalPages: 0,
        } as Paginated<UserDto>;
      }
      const result = await userService.search(searchQuery.value, 1, 10);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    }) as () => Promise<Paginated<UserDto>>,
    enabled: computed(() => !!searchQuery.value), // Only run query if searchQuery has a value
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const handleDataChange = (newVal: any) => {
    users.value = newVal?.items || [];
  };

  watch(data, handleDataChange, { immediate: true });

  return {
    state: {
      users,
      isLoading,
      isFetching,
      error,
    },
  };
}
