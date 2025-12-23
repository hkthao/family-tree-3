import { useQuery } from '@tanstack/vue-query';
import { computed, unref, type MaybeRefOrGetter } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import type { Prompt } from '@/types/prompt';
import type { IPromptService } from '@/services/prompt/prompt.service.interface';

export function usePromptQuery(promptId: MaybeRefOrGetter<string | undefined>) {
  const { prompt: promptService } = useServices();

  const queryResult = useQuery<Prompt | undefined, Error>({
    queryKey: ['prompt', promptId],
    queryFn: async () => {
      const id = unref(promptId);
      if (!id) return undefined;
      const result = await (promptService as IPromptService).getById(id as string);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    enabled: computed(() => !!unref(promptId)),
  });

  const prompt = computed(() => queryResult.data.value);
  const isLoading = computed(() => queryResult.isFetching.value);

  return {
    state: {
      prompt,
      isLoading,
      error: queryResult.error,
    },
    actions: {
      refetch: queryResult.refetch,
    },
  };
}
