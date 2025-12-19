import { useQuery } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { Ref } from 'vue';

export const useFamilyLocationQuery = (familyLocationId: Ref<string | undefined>) => {
  const { familyLocation: familyLocationService } = useServices();
  return useQuery({
    queryKey: ['familyLocation', familyLocationId],
    queryFn: async () => {
      if (!familyLocationId.value) return undefined;
      const response = await familyLocationService.getById(familyLocationId.value);
      if (response.ok) {
        return response.value;
      } else {
        throw new Error(response.error?.message || 'Failed to load family location');
      }
    },
    enabled: () => !!familyLocationId.value, // Only run the query if familyLocationId is available
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
};
