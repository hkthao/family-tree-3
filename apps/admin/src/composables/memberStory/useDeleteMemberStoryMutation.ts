import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables/utils/useServices';
import type { IMemberStoryService } from '@/services/memberStory/memberStory.service.interface'; // Import the interface

export function useDeleteMemberStoryMutation() {
  const { memberStory: memberStoryService } = useServices();
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: async (memberStoryId) => {
      const result = await (memberStoryService as IMemberStoryService).delete(memberStoryId);
      if (!result.ok) {
        throw result.error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['memberStories'] });
    },
  });
}
