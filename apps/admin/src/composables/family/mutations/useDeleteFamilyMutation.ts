import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/composables';



export function useDeleteFamilyMutation(service: IFamilyService = useServices().family) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (familyId: string) => {
      const response = await service.delete(familyId);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.families.all });
    },
  });
}