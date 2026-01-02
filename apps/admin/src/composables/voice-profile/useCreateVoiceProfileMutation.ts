import { useMutation } from '@tanstack/vue-query';

import type { IVoiceProfileService } from '@/services/voice-profile/voice-profile.service.interface';
import type { CreateVoiceProfileCommand, VoiceProfile } from '@/types';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types/apiError';

export function useCreateVoiceProfileMutation() {
  const { voiceProfile: voiceProfileService } = useServices();

  return useMutation<VoiceProfile, ApiError, CreateVoiceProfileCommand>({
    mutationFn: async (command: CreateVoiceProfileCommand) => {
      const response = await voiceProfileService.createVoiceProfile(command.memberId, command);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to create voice profile');
    },
  });
}
