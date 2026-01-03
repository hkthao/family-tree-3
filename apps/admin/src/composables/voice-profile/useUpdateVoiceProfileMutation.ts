import { useMutation } from '@tanstack/vue-query';

import type { UpdateVoiceProfileCommand, VoiceProfile } from '@/types';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types/apiError';

export function useUpdateVoiceProfileMutation() {
  const { voiceProfile: voiceProfileService } = useServices();

  return useMutation<VoiceProfile, ApiError, { id: string; data: UpdateVoiceProfileCommand }>({
    mutationFn: async ({ id, data }) => {
      const response = await voiceProfileService.update(data);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to update voice profile');
    },
  });
}
