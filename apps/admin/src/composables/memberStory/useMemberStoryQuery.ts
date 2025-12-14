import { useQuery } from '@tanstack/vue-query';
import { computed, unref, type MaybeRefOrGetter } from 'vue'; // Import unref, MaybeRefOrGetter
import { useServices } from '@/composables/utils/useServices';
import type { MemberStoryDto } from '@/types';
import type { IMemberStoryService } from '@/services/memberStory/memberStory.service.interface'; // Import the interface

export function useMemberStoryQuery(memberStoryId: MaybeRefOrGetter<string | undefined>) {
  const { memberStory: memberStoryService } = useServices();

  const queryResult = useQuery<MemberStoryDto | undefined, Error>({ // Added undefined to MemberStoryDto
    queryKey: ['memberStory', memberStoryId],
    queryFn: async () => {
      const id = unref(memberStoryId);
      if (!id) return undefined; // Handle undefined memberStoryId gracefully
      const result = await (memberStoryService as IMemberStoryService).getById(id as string);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    enabled: computed(() => !!unref(memberStoryId)), // Use computed for enabled
  });

  return queryResult;
}
