// apps/admin/src/composables/voice-profile/usePreprocessAndCreateVoiceProfileMutation.ts
import { useMutation } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { PreprocessAndCreateVoiceProfileCommand, VoiceProfileDto } from '@/types'; // Assuming these types are available
import type { Result } from '@/types';

export const usePreprocessAndCreateVoiceProfileMutation = () => {
  const services = useServices();

  return useMutation<Result<VoiceProfileDto>, Error, PreprocessAndCreateVoiceProfileCommand>({
    mutationFn: async (command: PreprocessAndCreateVoiceProfileCommand) => {
      const response = await services.voiceProfile.preprocessAndCreate(command);
      if (response.ok) {
        return response;
      }
      throw new Error(response.error?.message || 'Failed to preprocess and create voice profile');
    },
  });
};
