import { computed, ref, watch, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';
import type { UserDto } from '@/types';
import { queryKeys as QueryKeys } from '@/constants/queryKeys';

const userService = new ApiUserService(apiClient);

export function useUserByIdsQuery(ids: Readonly<Ref<string[]>>) {
  const users = ref<UserDto[]>([]);

  const { isLoading, isFetching, error, data } = useQuery<UserDto[], Error>({
    queryKey: QueryKeys.users.byIds(ids.value),
    queryFn: async () => {
      if (!ids.value || ids.value.length === 0) {
        return [];
      }
      const result = await userService.getByIds(ids.value);
      if (result.ok) {
        return result.value || [];
      }
      throw result.error;
    },
    enabled: computed(() => ids.value && ids.value.length > 0), // Only run query if ids has a value
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  watch(data, (newVal) => {
    users.value = newVal || [];
  }, { immediate: true });

  return {
    users,
    isLoading,
    isFetching,
    error,
  };
}
