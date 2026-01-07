import { computed } from 'vue'; // Removed toRef
import { useQuery } from '@tanstack/vue-query';
import { useUserPushTokenApi } from '@/composables/user-push-token/useUserPushTokenApi';
import type { Result, UserPushTokenDto, Paginated, ListOptions, FilterOptions } from '@/types';
import type { ApiError } from '@/types/apiError';
import type { Ref } from 'vue';

export function useUserPushTokensQuery(
  userId: Readonly<Ref<string | undefined | null>>,
  paginationOptions: ListOptions,
  filters: FilterOptions,
) {
  const { userPushTokenService } = useUserPushTokenApi();

  const queryResult = useQuery<Result<Paginated<UserPushTokenDto>, ApiError>, ApiError>({
    queryKey: ['user-push-tokens', userId, paginationOptions, filters],
    queryFn: () =>
      userPushTokenService.searchUserPushTokens(
        userId.value,
        paginationOptions,
        filters,
      ),
    enabled: computed(() => userId.value !== undefined), // Only enable query if userId is explicitly set or null (to fetch all)
  });

  const userPushTokens = computed(() => {
    const data = queryResult.data.value;
    return data?.ok ? data.value?.items || [] : [];
  });
  const totalItems = computed(() => {
    const data = queryResult.data.value;
    return data?.ok ? data.value?.totalItems || 0 : 0;
  });
  const isLoading = computed(() => queryResult.isLoading.value);
  const error = computed(() => queryResult.error.value);

  return {
    state: {
      userPushTokens,
      totalItems,
      isLoading,
      error,
    },
    actions: {
      refetch: queryResult.refetch,
    },
  };
}
