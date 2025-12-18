import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables';
import type { Prompt } from '@/types/prompt';
import type { IPromptService } from '@/services/prompt/prompt.service.interface';

export function useUpdatePromptMutation() {
  const { prompt: promptService } = useServices();
  const queryClient = useQueryClient();

  return useMutation<Prompt, Error, Prompt>({
    mutationFn: async (updatedPrompt) => {
      const result = await (promptService as IPromptService).update(updatedPrompt);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['prompts'] });
      queryClient.invalidateQueries({ queryKey: ['prompt', data.id] });
    },
  });
}
