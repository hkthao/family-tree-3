import { useMutation, useQueryClient } from '@tanstack/vue-query';

import { ApiFamilyDictService } from '@/services/family-dict/api.family-dict.service';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyDictService: IFamilyDictService = new ApiFamilyDictService(apiClient);

export function useDeleteFamilyDictMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: async (familyDictId: string) => {
      const response = await apiFamilyDictService.delete(familyDictId);
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