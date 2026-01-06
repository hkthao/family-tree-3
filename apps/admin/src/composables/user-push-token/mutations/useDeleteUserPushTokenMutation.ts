import { useMutation } from '@tanstack/vue-query';
import { useUserPushTokenApi } from '@/composables/user-push-token/useUserPushTokenApi';
import type { ApiError } from '@/types/apiError';
import type { Result } from '@/types';

export function useDeleteUserPushTokenMutation() {
  const { userPushTokenService } = useUserPushTokenApi();

  return useMutation<Result<void, ApiError>, ApiError, string>({
    mutationFn: (id: string) => userPushTokenService.delete(id),
  });
}
