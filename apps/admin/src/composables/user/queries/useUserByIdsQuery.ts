import { ref, watch, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { UserDto } from '@/types';
import { queryKeys as QueryKeys } from '@/constants/queryKeys';
import type { IUserService } from '@/services/user/user.service.interface';
import { useServices } from '@/plugins/services.plugin';

interface UseUserByIdsQueryOptions {
  userService?: IUserService;
}

export function useUserByIdsQuery(ids: Readonly<Ref<string[]>>, options: UseUserByIdsQueryOptions = {}) {
  const { userService = useServices().user } = options;
  const users = ref<UserDto[]>([]);

  // Explicitly clear users if ids array is empty
  if (ids.value.length === 0) {
    users.value = [];
  }

  const { isLoading, isFetching, error, data } = useQuery<UserDto[], Error>({
    queryKey: QueryKeys.users.byIds(ids.value),
    queryFn: async () => {
      if (ids.value.length === 0) {
        return [];
      }
      const result = await userService.getByIds(ids.value);
      if (result.ok) {
        return result.value || [];
      }
      throw result.error;
    },
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const handleDataChange = (newVal?: UserDto[]) => {
    users.value = newVal || [];
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
