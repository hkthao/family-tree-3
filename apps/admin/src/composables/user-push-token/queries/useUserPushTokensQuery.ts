import { computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useUserPushTokenApi } from '@/composables/user-push-token/useUserPushTokenApi';
import type { Result, UserPushTokenDto } from '@/types';
import type { ApiError } from '@/types/apiError';
import type { Ref } from 'vue';

export function useUserPushTokensQuery(userId: Readonly<Ref<string>>) {
  const { userPushTokenService } = useUserPushTokenApi();

  const queryResult = useQuery<Result<UserPushTokenDto[], ApiError>, ApiError>({
    queryKey: ['user-push-tokens', userId],
    queryFn: () => userPushTokenService.getUserPushTokensByUserId(userId.value),
    enabled: computed(() => !!userId.value), // Only run query if userId is available
  });

  const userPushTokens = computed(() => {
    const data = queryResult.data.value;
    return data?.ok ? data.value : [];
  });
  const isLoading = computed(() => queryResult.isLoading.value);
  const error = computed(() => queryResult.error.value);

  return {
    state: {
      userPushTokens,
      isLoading,
      error,
    },
    actions: {
      refetch: queryResult.refetch,
    },
  };
}
