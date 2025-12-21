import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import type { FamilyUpdateDto } from '@/types'; // Updated import
import { useServices } from '@/composables';



export function useUpdateFamilyMutation(service: IFamilyService = useServices().family) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (updatedFamily: FamilyUpdateDto) => {
      if (!updatedFamily.id) {
        throw new Error('Family ID is required for update');
      }
      const response = await service.update(updatedFamily);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: (_data, variables) => {
      // Invalidate all families list query to refetch latest data
      queryClient.invalidateQueries({ queryKey: queryKeys.families.all });
      // Invalidate specific family detail query to refetch latest data
      queryClient.invalidateQueries({ queryKey: queryKeys.families.detail(variables.id!) });
    },
  });
}