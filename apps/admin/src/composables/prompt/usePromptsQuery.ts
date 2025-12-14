import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { reactive, watch } from 'vue';
import { useServices } from '@/composables/utils/useServices';
import type { Prompt } from '@/types/prompt';
import type { Paginated, ListOptions } from '@/types/pagination.d';
import type { IPromptService } from '@/services/prompt/prompt.service.interface';

export interface PromptListOptions extends ListOptions {
  searchQuery?: string;
}

export function usePromptsQuery(options: PromptListOptions) {
  const { prompt: promptService } = useServices();
  const queryClient = useQueryClient();

  const reactiveOptions = reactive(options);

  const queryResult = useQuery<Paginated<Prompt>, Error>({
    queryKey: ['prompts', reactiveOptions],
    queryFn: async () => {
      const { searchQuery, ...listOptions } = reactiveOptions;
      const filters = { searchQuery };
      const result = await (promptService as IPromptService).search(listOptions, filters);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
  });

  watch(reactiveOptions, () => {
    queryClient.invalidateQueries({ queryKey: ['prompts'] });
  }, { deep: true });

  return queryResult;
}
