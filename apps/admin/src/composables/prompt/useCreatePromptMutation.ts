import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables/utils/useServices';
import type { Prompt } from '@/types/prompt';
import type { IPromptService } from '@/services/prompt/prompt.service.interface';

export function useCreatePromptMutation() {
  const { prompt: promptService } = useServices();
  const queryClient = useQueryClient();

  return useMutation<Prompt, Error, Omit<Prompt, 'id'>>({
    mutationFn: async (newPrompt) => {
      const result = await (promptService as IPromptService).add(newPrompt);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['prompts'] });
    },
  });
}
