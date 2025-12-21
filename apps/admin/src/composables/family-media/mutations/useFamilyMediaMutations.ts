import type { FamilyMedia } from '@/types';
import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables'; // Import useServices
import type { IFamilyMediaService } from '@/services/family-media/family-media.service.interface';
import { queryKeys } from '@/constants/queryKeys';



/**
 * Composible để thêm mới Family Media.
 * @returns useMutation hook cho việc thêm media.
 */
export function useAddFamilyMediaMutation(service: IFamilyMediaService = useServices().familyMedia) {
  const queryClient = useQueryClient();
  return useMutation<FamilyMedia, Error, { familyId: string; file: File; description?: string }>({
    mutationFn: async (input: { familyId: string; file: File; description?: string }) => {
      const response = await service.create(input.familyId, input.file, input.description);
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
export function useDeleteFamilyMediaMutation(service: IFamilyMediaService = useServices().familyMedia) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id }: { familyId: string; id: string }) => {
      const response = await service.delete(id);
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
