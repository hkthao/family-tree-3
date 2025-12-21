import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/composables';



export function useDeleteFamilyDictMutation(service: IFamilyDictService = useServices().familyDict) {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: async (familyDictId: string) => {
      const response = await service.delete(familyDictId);
      if (response.ok) {
        return;
      }
      throw response.error;
    },
    onSuccess: () => {
      // Invalidate and refetch all family dicts
      queryClient.invalidateQueries({ queryKey: queryKeys.familyDicts.all });
    },
  });
}