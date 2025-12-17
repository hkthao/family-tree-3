import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin'; // Correct import

export const useDeleteMemoryItemMutation = () => {
  const services = useServices(); // Correct way to access services
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  return useMutation<void, Error, { familyId: string; id: string }>({
    mutationFn: async ({ familyId, id }) => {
      const result = await services.memoryItem.deleteMemoryItem(familyId, id);
      if (result.ok) {
        return result.value;
      }
      showSnackbar(result.error?.message || t('memoryItem.messages.deleteError'), 'error');
      throw result.error;
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['family', variables.familyId, 'memory-items'],
      });
      // Optionally, invalidate detail query for the deleted item
      // queryClient.invalidateQueries({ queryKey: ['family', variables.familyId, 'memory-item', variables.id] });
    },
  });
};
