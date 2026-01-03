import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyMedia, FamilyMediaAddFromUrlDto, ApiError } from '@/types';
import { queryKeys } from '@/constants/queryKeys';

export const useAddFamilyMediaFromUrlMutation = () => {
  const services = useServices();
  const queryClient = useQueryClient();

  return useMutation<FamilyMedia, ApiError, FamilyMediaAddFromUrlDto>({
    mutationFn: async (payload) => {
      const response = await services.familyMedia.addFromUrl(payload.familyId, payload);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.familyMedia.list({}, { familyId: variables.familyId }) });
    },
  });
};
