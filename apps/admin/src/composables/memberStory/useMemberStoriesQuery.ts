import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { reactive, watch } from 'vue';
import { useServices } from '@/composables/utils/useServices';
import type { MemberStoryDto } from '@/types';
import type { Paginated, ListOptions } from '@/types/pagination.d';
import type { IMemberStoryService } from '@/services/memberStory/memberStory.service.interface'; // Import the interface

export interface MemberStoryListOptions extends ListOptions {
  searchQuery?: string;
  memberId?: string;
  familyId?: string;
}

export function useMemberStoriesQuery(options: MemberStoryListOptions) {
  const { memberStory: memberStoryService } = useServices();
  const queryClient = useQueryClient();

  const reactiveOptions = reactive(options);

  const queryResult = useQuery<Paginated<MemberStoryDto>, Error>({
    queryKey: ['memberStories', reactiveOptions],
    queryFn: async () => {
      const { searchQuery, memberId, familyId, ...listOptions } = reactiveOptions;
      const filters = { searchQuery, memberId, familyId };
      const result = await (memberStoryService as IMemberStoryService).search(listOptions, filters);
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
