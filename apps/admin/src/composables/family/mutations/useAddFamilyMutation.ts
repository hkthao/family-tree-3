import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ApiFamilyService } from '@/services/family/api.family.service';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';
import type { Family } from '@/types';

const apiFamilyService: IFamilyService = new ApiFamilyService(apiClient);

export function useAddFamilyMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (familyData: Omit<Family, 'id'>) => {
      const response = await apiFamilyService.add(familyData);
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