import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import type { FamilyDictImport } from '@/types';
import { useServices } from '@/composables';



export function useImportFamilyDictMutation(service: IFamilyDictService = useServices().familyDict) {
  const queryClient = useQueryClient();

  return useMutation<string[], Error, FamilyDictImport>({
    mutationFn: async (data: FamilyDictImport) => {
      const response = await service.importItems(data);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.familyDicts.all });
    },
  });
}