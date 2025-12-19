import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';

export const useDeleteMemoryItemMutation = () => {
  const services = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  return useMutation<void, Error, { familyId: string; id: string }>({
    mutationFn: async ({ id }) => {
      const result = await services.memoryItem.delete(id);
      if (result.ok) {
        return result.value;
      }
      showSnackbar(result.error?.message || t('memoryItem.messages.deleteError'), 'error');
      throw result.error;
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['memory-items', { familyId: variables.familyId }],
      });
      queryClient.invalidateQueries({
        queryKey: ['memory-items', variables.id],
        exact: true
      });
    },
  });
};
