import { useMutation } from '@tanstack/vue-query';

import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types/apiError';

export function useDeleteVoiceProfileMutation() {
  const { voiceProfile: voiceProfileService } = useServices();

  return useMutation<void, ApiError, { id: string; memberId: string }>({
    mutationFn: async ({ id, memberId }) => {
      const response = await voiceProfileService.delete(id);
      if (!response.ok) {
        throw new Error(response.error?.message || 'Failed to delete voice profile');
      }
    },
  });
}
