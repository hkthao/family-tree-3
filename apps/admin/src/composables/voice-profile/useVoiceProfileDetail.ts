import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { VoiceProfile } from '@/types';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types/apiError'; // Import ApiError

interface UseVoiceProfileDetailOptions {
  voiceProfileId: Ref<string>;
  onClose: () => void;
}

export function useVoiceProfileDetail(options: UseVoiceProfileDetailOptions) {
  const { voiceProfile: voiceProfileService } = useServices();

  const queryResult = useQuery<VoiceProfile, ApiError>({
    queryKey: ['voice-profile', unref(options.voiceProfileId)],
    queryFn: async () => {
      const response = await voiceProfileService.getById(unref(options.voiceProfileId));
      if (response.ok) {
        if (response.value) {
          return response.value;
        }
        throw new Error('Voice profile not found');
      }
      throw new Error(response.error?.message || 'Failed to fetch voice profile details');
    },
    enabled: computed(() => !!unref(options.voiceProfileId)),
  });

  const voiceProfile = computed<VoiceProfile | null>(() => queryResult.data.value ?? null);
  const isLoading = computed<boolean>(() => queryResult.isPending.value);
  const error = computed<Error | null>(() => queryResult.error.value);

  const closeView = () => {
    options.onClose();
  };

  return {
    state: {
      voiceProfile,
      isLoading,
      error,
    },
    actions: {
      closeView,
      refetch: queryResult.refetch,
    },
  };
}
