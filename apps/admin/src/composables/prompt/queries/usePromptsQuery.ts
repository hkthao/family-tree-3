import { useQuery } from '@tanstack/vue-query';
import { reactive, computed } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import type { Prompt } from '@/types/prompt';
import type { Paginated, ListOptions } from '@/types/pagination.d';
import type { IPromptService } from '@/services/prompt/prompt.service.interface';

export interface PromptListOptions extends ListOptions {
  searchQuery?: string;
}

export function usePromptsQuery(options: PromptListOptions) {
  const { prompt: promptService } = useServices();


  const reactiveOptions = reactive(options);

  const queryResult = useQuery<Paginated<Prompt>, Error>({
    queryKey: [
      'prompts',
      reactiveOptions.page,
      reactiveOptions.itemsPerPage,
      reactiveOptions.sortBy,
      reactiveOptions.searchQuery,
    ],
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

  const prompts = computed(() => queryResult.data.value?.items || []);
  const totalItems = computed(() => queryResult.data.value?.totalItems || 0);
  const isLoading = computed(() => queryResult.isFetching.value);

  return {
    state: {
      prompts,
      totalItems,
      isLoading,
      error: queryResult.error,
    },
    actions: {
      refetch: queryResult.refetch,
    },
  };
}
