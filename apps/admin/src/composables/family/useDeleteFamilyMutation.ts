import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { ApiFamilyService } from '@/services/family/api.family.service';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyService: IFamilyService = new ApiFamilyService(apiClient);

export function useDeleteFamilyMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (familyId: string) => {
      const response = await apiFamilyService.delete(familyId);
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