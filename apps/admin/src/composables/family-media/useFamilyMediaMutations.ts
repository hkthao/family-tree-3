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
  return useMutation({
    mutationFn: async ({ familyId, file, description }: { familyId: string; file: File; description?: string }) => {
      const response = await apiFamilyMediaService.create(familyId, file, description);
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
 * Composible để cập nhật Family Media.
 * TODO: Implement backend API for updating family media metadata.
 * @returns useMutation hook cho việc cập nhật media.
 */
export function useUpdateFamilyMediaMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ familyId, mediaId, description }: { familyId: string; mediaId: string; description?: string }) => {
      // TODO: Implement the actual API call to update family media metadata
      // The current ApiFamilyMediaService does not have a dedicated 'update' method for metadata.
      // This is a placeholder and assumes an update endpoint exists or will be created.
      console.warn('useUpdateFamilyMediaMutation: Actual API call for update is not implemented yet.');
      console.log(`Simulating update for media ${mediaId} in family ${familyId} with description: ${description}`);
      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 500));
      return 'Simulated update success'; // Return a successful value
    },
    onSuccess: () => {
      // Invalidate relevant queries to refetch data after updating
      queryClient.invalidateQueries({ queryKey: queryKeys.familyMedia.all });
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
    mutationFn: async ({ familyId, id }: { familyId: string; id: string }) => {
      const response = await apiFamilyMediaService.delete(familyId, id);
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
