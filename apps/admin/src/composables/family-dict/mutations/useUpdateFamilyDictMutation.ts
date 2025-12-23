import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { FamilyDict } from '@/types';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';



export function useUpdateFamilyDictMutation(service: IFamilyDictService = useServices().familyDict) {
  const queryClient = useQueryClient();

  return useMutation<FamilyDict, Error, FamilyDict>({
    mutationFn: async (updatedFamilyDict: FamilyDict) => {
      const response = await service.update(updatedFamilyDict);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: (data) => {
      // Invalidate and refetch all family dicts
      queryClient.invalidateQueries({ queryKey: queryKeys.familyDicts.all });
      // Invalidate and refetch the specific family dict detail
      queryClient.invalidateQueries({ queryKey: queryKeys.familyDicts.detail(data.id) });
    },
  });
}