import { computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useUserPushTokenApi } from '@/composables/user-push-token/useUserPushTokenApi';
import type { Result, UserPushTokenDto } from '@/types';
import type { ApiError } from '@/types/apiError';
import type { Ref } from 'vue';

export function useUserPushTokenDetailQuery(userPushTokenId: Readonly<Ref<string>>) {
  const { userPushTokenService } = useUserPushTokenApi();

  const queryResult = useQuery<Result<UserPushTokenDto | undefined, ApiError>, ApiError>({
    queryKey: ['user-push-token', userPushTokenId],
    queryFn: () => userPushTokenService.getById(userPushTokenId.value),
    enabled: computed(() => !!userPushTokenId.value), // Only run query if userPushTokenId is available
  });

  const userPushToken = computed(() => {
    const data = queryResult.data.value;
    return data?.ok ? data.value : undefined;
  });
  const isLoading = computed(() => queryResult.isLoading.value);
  const error = computed(() => queryResult.error.value);

  return {
    state: {
      userPushToken,
      isLoading,
      error,
    },
    actions: {
      refetch: queryResult.refetch,
    },
  };
}
