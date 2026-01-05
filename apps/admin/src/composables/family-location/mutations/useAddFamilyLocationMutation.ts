import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { AddFamilyLocationDto } from '@/types';

export const useAddFamilyLocationMutation = () => {
  const queryClient = useQueryClient();
  const { familyLocation: familyLocationService } = useServices();
  return useMutation({
    mutationFn: async (newFamilyLocation: AddFamilyLocationDto) => {
      const response = await familyLocationService.add(newFamilyLocation);
      if (response.ok) {
        return response.value;
      } else {
        throw new Error(response.error?.message || 'Failed to add family location');
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['familyLocations', 'list'] });
    },
  });
};
