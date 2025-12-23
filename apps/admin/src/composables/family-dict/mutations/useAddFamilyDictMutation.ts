import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { FamilyDict } from '@/types';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';



export function useAddFamilyDictMutation(service: IFamilyDictService = useServices().familyDict) {
  const queryClient = useQueryClient();

  return useMutation<FamilyDict, Error, Omit<FamilyDict, 'id'>>({
    mutationFn: async (newFamilyDict: Omit<FamilyDict, 'id'>) => {
      const response = await service.add(newFamilyDict);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: () => {
      // Invalidate and refetch all family dicts
      queryClient.invalidateQueries({ queryKey: queryKeys.familyDicts.all });
    },
  });
}