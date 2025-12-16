import type { FamilyMedia } from '@/types';
import { useMutation, useQueryClient } from '@tanstack/vue-query';

import { ApiFamilyMediaService } from '@/services/family-media/api.family-media.service';
import type { IFamilyMediaService } from '@/services/family-media/family-media.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyMediaService: IFamilyMediaService = new ApiFamilyMediaService(apiClient);

/**
 * Composible để thêm mới Family Media.
 * @returns useMutation hook cho việc thêm media.
 */
export function useAddFamilyMediaMutation() {
  const queryClient = useQueryClient();
  return useMutation<FamilyMedia, Error, { familyId: string; file: File; description?: string }>({
    mutationFn: async (input: { familyId: string; file: File; description?: string }) => {
      const response = await apiFamilyMediaService.create(input.familyId, input.file, input.description);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: () => {
      // Invalidate relevant queries to refetch data after adding
      queryClient.invalidateQueries({ queryKey: queryKeys.familyMedia.all });
    },
  });
}



/**
 * Composible để xóa Family Media.
 * @returns useMutation hook cho việc xóa media.
 */
export function useDeleteFamilyMediaMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id }: { familyId: string; id: string }) => {
      const response = await apiFamilyMediaService.delete(id);
      if (response.ok) {
        return response.value;
      }
      throw response.error;
    },
    onSuccess: () => {
      // Invalidate relevant queries to refetch data after deleting
      queryClient.invalidateQueries({ queryKey: queryKeys.familyMedia.all });
    },
  });
}
