import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables/utils/useServices';
import type { MemberStoryDto } from '@/types';
import type { IMemberStoryService } from '@/services/memberStory/memberStory.service.interface'; // Import the interface

export function useUpdateMemberStoryMutation() {
  const { memberStory: memberStoryService } = useServices();
  const queryClient = useQueryClient();

  return useMutation<MemberStoryDto, Error, MemberStoryDto>({
    mutationFn: async (updatedStory) => {
      const result = await (memberStoryService as IMemberStoryService).update(updatedStory);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['memberStories'] });
      queryClient.invalidateQueries({ queryKey: ['memberStory', data.id] });
    },
  });
}
