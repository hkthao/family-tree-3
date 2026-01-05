import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyLocation, UpdateFamilyLocationDto } from '@/types';

export const useUpdateFamilyLocationMutation = () => {
  const queryClient = useQueryClient();
  const { familyLocation: familyLocationService } = useServices();
  return useMutation({
    mutationFn: async (updatedFamilyLocation: UpdateFamilyLocationDto) => {
      const response = await familyLocationService.update(updatedFamilyLocation);
      if (response.ok) {
        return response.value;
      } else {
        throw new Error(response.error?.message || 'Failed to update family location');
      }
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['familyLocations', 'list'] });
      queryClient.invalidateQueries({ queryKey: ['familyLocation', variables.id] });
    },
  });
};
