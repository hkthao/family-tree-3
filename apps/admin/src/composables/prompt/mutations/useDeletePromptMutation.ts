import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useServices } from '@/composables';
import type { IPromptService } from '@/services/prompt/prompt.service.interface';

export function useDeletePromptMutation() {
  const { prompt: promptService } = useServices();
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: async (promptId) => {
      const result = await (promptService as IPromptService).delete(promptId);
      if (!result.ok) {
        throw result.error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['prompts'] });
    },
  });
}
