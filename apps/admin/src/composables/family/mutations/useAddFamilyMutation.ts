import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import type { FamilyAddDto } from '@/types'; // Updated import
import { useServices } from '@/plugins/services.plugin';


export function useAddFamilyMutation(service: IFamilyService = useServices().family) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (familyData: FamilyAddDto) => {
      const response = await service.add(familyData);
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