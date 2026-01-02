import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';

import type { IVoiceProfileService } from '@/services/voice-profile/voice-profile.service.interface';
import type { VoiceProfile, Paginated } from '@/types';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError } from '@/types/apiError'; // Import ApiError

export function useVoiceProfilesQuery(
  memberId: Ref<string>,
  paginationOptions: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] },
  filters: { search: string }
) {
  const { voiceProfile: voiceProfileService } = useServices();

  const queryResult = useQuery<Paginated<VoiceProfile>, ApiError>({
    queryKey: [
      'voice-profiles',
      {
        memberId: unref(memberId),
        page: computed(() => paginationOptions.page),
        itemsPerPage: computed(() => paginationOptions.itemsPerPage),
        sortBy: computed(() => paginationOptions.sortBy),
        search: computed(() => filters.search),
      },
    ],
    queryFn: async () => {
      const response = await voiceProfileService.getVoiceProfilesByMemberId(
        unref(memberId),
        paginationOptions.page,
        paginationOptions.itemsPerPage,
        filters.search,
        paginationOptions.sortBy[0]?.key,
        paginationOptions.sortBy[0]?.order
      );
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to fetch voice profiles');
    },
    enabled: computed(() => !!unref(memberId)),
  });

  const voiceProfiles = computed<VoiceProfile[]>(() => queryResult.data.value?.items || []);
  const totalItems = computed<number>(() => queryResult.data.value?.totalItems || 0);
  const isLoading = computed<boolean>(() => queryResult.isPending.value);
  const error = computed<ApiError | null>(() => queryResult.error.value); // Use ApiError here

  return {
    state: {
      voiceProfiles,
      totalItems,
      isLoading,
      error,
    },
    actions: {
      refetch: queryResult.refetch,
    },
  };
}
