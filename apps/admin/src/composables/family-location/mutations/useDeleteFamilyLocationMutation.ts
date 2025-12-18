import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';

export const useDeleteFamilyLocationMutation = () => {
  const queryClient = useQueryClient();
  const { familyLocation: familyLocationService } = useServices();
  return useMutation({
    mutationFn: async (familyLocationId: string) => {
      const response = await familyLocationService.delete(familyLocationId);
      if (response.ok) {
        return response.value;
      } else {
        throw new Error(response.error?.message || 'Failed to delete family location');
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['familyLocations', 'list'] });
    },
  });
};
