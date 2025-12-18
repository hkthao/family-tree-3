import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { MemoryItem } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useUpdateMemoryItemMutation = () => {
  const services = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  return useMutation<MemoryItem, Error, MemoryItem>({
    mutationFn: async (updatedMemoryItem) => {
      if (!updatedMemoryItem.familyId) {
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      const result = await services.memoryItem.update(updatedMemoryItem);
      if (result.ok) {
        return result.value;
      }
      showSnackbar(result.error?.message || t('memoryItem.messages.saveError'), 'error');
      throw result.error;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['memory-items', { familyId: data.familyId }],
      });
      queryClient.invalidateQueries({
        queryKey: ['memory-items', data.id],
        exact: true
      });
    },
  });
};
