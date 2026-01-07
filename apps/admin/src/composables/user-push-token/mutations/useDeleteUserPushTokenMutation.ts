import { useMutation } from '@tanstack/vue-query';
import { useUserPushTokenApi } from '@/composables/user-push-token/useUserPushTokenApi';
import type { ApiError } from '@/types/apiError';
import type { Result } from '@/types';



export function useDeleteUserPushTokenMutation() {
  const { userPushTokenService } = useUserPushTokenApi();

  return useMutation<void, ApiError, string>({ // Change TData from Result<void, ApiError> to void
    mutationFn: async (id: string) => { // Make mutationFn async
      const result = await userPushTokenService.delete(id);
      if (result.ok) {
        return result.value; // Resolve with void
      } else {
        throw result.error; // Reject with ApiError
      }
    },
  });
}
