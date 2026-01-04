import { computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { VoiceGenerationDto } from '@/types';
import type { Ref } from 'vue';

interface VoiceGenerationHistoryFilters {
  voiceProfileId: string;
}

export function useVoiceGenerationHistoryQuery(
  filters: Ref<VoiceGenerationHistoryFilters>,
) {
  const { voiceProfile: voiceProfileService } = useServices();

  const queryResult = useQuery<VoiceGenerationDto[]>({
    queryKey: ['voice-generation-history', filters],
    queryFn: async () => {
      const response = await voiceProfileService.getVoiceGenerationHistory(
        filters.value.voiceProfileId
      );
      if (!response.ok) {
        throw new Error(response.error?.message || 'Failed to fetch voice generation history');
      }
      return response.value;
    },
    enabled: computed(() => !!filters.value.voiceProfileId),
  });

  const voiceGenerations = computed(() => queryResult.data.value || []);

  return {
    ...queryResult,
    voiceGenerations,
  };
}
