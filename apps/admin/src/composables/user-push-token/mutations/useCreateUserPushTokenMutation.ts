import { useMutation } from '@tanstack/vue-query';
import { useUserPushTokenApi } from '@/composables/user-push-token/useUserPushTokenApi';
import type { ApiError } from '@/types/apiError';
import type { CreateUserPushTokenCommand, UserPushTokenDto, Result } from '@/types';

export function useCreateUserPushTokenMutation() {
  const { userPushTokenService } = useUserPushTokenApi();

  return useMutation<Result<UserPushTokenDto, ApiError>, ApiError, CreateUserPushTokenCommand>({
    mutationFn: (command: CreateUserPushTokenCommand) =>
      userPushTokenService.add(command),
  });
}
