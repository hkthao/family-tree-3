import { useMutation, useQueryClient } from '@tanstack/vue-query';
import type { FamilyDict } from '@/types';
import { ApiFamilyDictService } from '@/services/family-dict/api.family-dict.service';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyDictService: IFamilyDictService = new ApiFamilyDictService(apiClient);

export function useUpdateFamilyDictMutation() {
  const queryClient = useQueryClient();

  return useMutation<FamilyDict, Error, FamilyDict>({
    mutationFn: async (updatedFamilyDict: FamilyDict) => {
      const response = await apiFamilyDictService.update(updatedFamilyDict);
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