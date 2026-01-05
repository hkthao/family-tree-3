import { unref, type Ref, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { LocationLinkDto } from '@/types/location-link'; // Use the new type
import { fetchLocationLinksByMemberId } from '@/services/location-link/locationLink.service'; // Import the new service

export function useLocationLinksByMemberIdQuery(memberIdRef: Ref<string | null>) {
  const memberId = unref(memberIdRef); // Get the current value of memberIdRef

  const queryKey = ['locationLinks', 'byMember', memberId];

  const {
    data,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: queryKey,
    queryFn: async (): Promise<LocationLinkDto[]> => {
      if (!memberId) {
        return [];
      }
      return await fetchLocationLinksByMemberId(memberId);
    },
    enabled: computed(() => !!unref(memberIdRef)), // Only run query if memberId is available
  });

  return {
    data,
    isLoading,
    error,
    refetch,
  };
}
