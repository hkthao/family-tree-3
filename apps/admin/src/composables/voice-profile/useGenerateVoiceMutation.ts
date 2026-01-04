import { useMutation } from '@tanstack/vue-query';

import { useServices } from '@/plugins/services.plugin';
import type { ApiError, VoiceGenerationDto } from '@/types'; // Assuming VoiceGenerationDto is defined in '@/types'

interface GenerateVoicePayload {
  voiceProfileId: string;
  text: string;
}

export function useGenerateVoiceMutation() {
  const { voiceProfile: voiceProfileService } = useServices();

  return useMutation<VoiceGenerationDto, ApiError, GenerateVoicePayload>({
    mutationFn: async ({ voiceProfileId, text }) => {
      const response = await voiceProfileService.generateVoice(voiceProfileId, text);
      if (!response.ok) {
        throw new Error(response.error?.message || 'Failed to generate voice');
      }
      return response.value;
    },
  });
}
