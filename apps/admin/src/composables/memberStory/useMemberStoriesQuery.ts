import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { reactive, watch } from 'vue';
import { useServices } from '@/composables/utils/useServices';
import type { MemberStoryDto } from '@/types';
import type { Paginated } from '@/types/pagination.d'; // ListOptions is now part of SearchMemberStoriesRequest
import type { IMemberStoryService } from '@/services/memberStory/memberStory.service.interface'; // Import the interface
import type { SearchMemberStoriesRequest } from '@/types/memberStory.d'; // NEW import

export function useMemberStoriesQuery(options: SearchMemberStoriesRequest) {
  const { memberStory: memberStoryService } = useServices();
  const queryClient = useQueryClient();

  const reactiveOptions = reactive(options);

  const queryResult = useQuery<Paginated<MemberStoryDto>, Error>({
    queryKey: ['memberStories', reactiveOptions],
    queryFn: async () => {
      // Directly pass reactiveOptions for ListOptions since SearchMemberStoriesRequest extends ListOptions
      // Extract filters explicitly
      const { searchQuery, memberId, familyId } = reactiveOptions;
      const filters = { searchQuery, memberId, familyId };
      const result = await (memberStoryService as IMemberStoryService).search(reactiveOptions, filters); // Pass reactiveOptions for listOptions
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
  });

  watch(reactiveOptions, () => {
    queryClient.invalidateQueries({ queryKey: ['memberStories'] });
  }, { deep: true });

  return queryResult;
}
