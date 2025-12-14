import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables/utils/useServices';
import type { MemberStoryDto } from '@/types';
import type { IMemberStoryService } from '@/services/memberStory/memberStory.service.interface'; // Import the interface for type safety

export function useCreateMemberStoryMutation() {
  const { memberStory: memberStoryService } = useServices();
  const queryClient = useQueryClient();

  return useMutation<MemberStoryDto, Error, Omit<MemberStoryDto, 'id'>>({
    mutationFn: async (newStory) => {
      const result = await (memberStoryService as IMemberStoryService).add(newStory); // Explicit cast to IMemberStoryService
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['memberStories'] });
    },
  });
}
