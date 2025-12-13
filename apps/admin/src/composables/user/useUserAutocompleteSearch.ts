import { ref, watch, computed, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';
import type { UserDto, Paginated } from '@/types';
import { queryKeys as QueryKeys } from '@/constants/queryKeys';

const userService = new ApiUserService(apiClient);

export function useUserAutocompleteSearch(searchQuery: Readonly<Ref<string>>) {
  const users = ref<UserDto[]>([]);

  const { isLoading, isFetching, error, data } = useQuery<Paginated<UserDto>, Error>({
    queryKey: QueryKeys.users.search(searchQuery.value),
    queryFn: async () => {
      if (!searchQuery.value) {
        return {
          items: [],
          page: 1,
          totalItems: 0,
          pageNumber: 1,
          pageSize: 10,
          totalCount: 0,
          totalPages: 0,
          hasPreviousPage: false,
          hasNextPage: false,
        };
      }
      const result = await userService.search(searchQuery.value, 1, 10);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    enabled: computed(() => !!searchQuery.value), // Only run query if searchQuery has a value
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  watch(data, (newVal) => {
    users.value = newVal?.items || [];
  }, { immediate: true });

  return {
    users,
    isLoading,
    isFetching,
    error,
  };
}
