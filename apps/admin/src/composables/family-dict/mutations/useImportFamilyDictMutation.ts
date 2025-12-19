import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ApiFamilyDictService } from '@/services/family-dict/api.family-dict.service';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';
import type { FamilyDictImport } from '@/types';

const apiFamilyDictService: IFamilyDictService = new ApiFamilyDictService(apiClient);

export function useImportFamilyDictMutation() {
  const queryClient = useQueryClient();

  return useMutation<string[], Error, FamilyDictImport>({
    mutationFn: async (data: FamilyDictImport) => {
      const response = await apiFamilyDictService.importItems(data);
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