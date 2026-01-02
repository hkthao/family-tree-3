import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';

import type { VoiceProfile, Paginated, ListOptions, FilterOptions } from '@/types'; // Add ListOptions, FilterOptions
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
        queryType: 'search', // Updated queryType
      },
    ],
    queryFn: async () => {
      const options: ListOptions = {
        page: paginationOptions.page,
        itemsPerPage: paginationOptions.itemsPerPage,
        sortBy: paginationOptions.sortBy.map(item => ({
          key: item.key,
          order: item.order === 'desc' ? 'desc' : 'asc', // Ensure order is 'asc' or 'desc'
        })),
      };
      const queryFilters: FilterOptions = {
        search: filters.search,
        memberId: unref(memberId), // Pass memberId as a filter
      };
      const response = await voiceProfileService.search(options, queryFilters);
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