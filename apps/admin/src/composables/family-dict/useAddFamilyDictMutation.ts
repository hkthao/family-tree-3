import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { FamilyDict } from '@/types';
import { ApiFamilyDictService } from '@/services/family-dict/api.family-dict.service';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyDictService: IFamilyDictService = new ApiFamilyDictService(apiClient);

export function useAddFamilyDictMutation() {
  const queryClient = useQueryClient();

  return useMutation<FamilyDict, Error, Omit<FamilyDict, 'id'>>({
    mutationFn: async (newFamilyDict: Omit<FamilyDict, 'id'>) => {
      const response = await apiFamilyDictService.add(newFamilyDict);
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