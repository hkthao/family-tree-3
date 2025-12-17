import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { MemoryItem } from '@/types';
import { useServices } from '@/plugins/services.plugin'; // Correct import

export const useUpdateMemoryItemMutation = () => {
  const services = useServices(); // Correct way to access services
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  return useMutation<MemoryItem, Error, Omit<MemoryItem, 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>>({
    mutationFn: async (updatedMemoryItem) => {
      if (!updatedMemoryItem.familyId) {
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      const result = await services.memoryItem.updateMemoryItem(
        updatedMemoryItem.familyId,
        updatedMemoryItem,
      );
      if (result.ok) {
        return result.value;
      }
      showSnackbar(result.error?.message || t('memoryItem.messages.saveError'), 'error');
      throw result.error;
    },
    onSuccess: (data, variables) => {
      // Invalidate list query
      queryClient.invalidateQueries({
        queryKey: ['family', variables.familyId, 'memory-items'],
      });
      // Invalidate detail query for the updated item
      queryClient.invalidateQueries({
        queryKey: ['family', variables.familyId, 'memory-item', variables.id],
      });
    },
  });
};
