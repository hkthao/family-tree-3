import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ApiFamilyService } from '@/services/family/api.family.service';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';
import type { FamilyUpdateDto } from '@/types'; // Updated import

const apiFamilyService: IFamilyService = new ApiFamilyService(apiClient);

export function useUpdateFamilyMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (updatedFamily: FamilyUpdateDto) => {
      if (!updatedFamily.id) {
        throw new Error('Family ID is required for update');
      }
      const response = await apiFamilyService.update(updatedFamily);
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