import { useMutation } from '@tanstack/vue-query';
import { useUserPushTokenApi } from '@/composables/user-push-token/useUserPushTokenApi';
import type { ApiError } from '@/types/apiError';
import type { UpdateUserPushTokenCommand, UserPushTokenDto, Result } from '@/types';

export function useUpdateUserPushTokenMutation() {
  const { userPushTokenService } = useUserPushTokenApi();

  return useMutation<Result<UserPushTokenDto, ApiError>, ApiError, UpdateUserPushTokenCommand>({
    mutationFn: (command: UpdateUserPushTokenCommand) =>
      userPushTokenService.update(command),
  });
}
