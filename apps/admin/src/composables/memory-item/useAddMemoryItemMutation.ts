import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { MemoryItem } from '@/types';
import { useServices } from '@/plugins/services.plugin'; // Correct import

export const useAddMemoryItemMutation = () => {
  const services = useServices(); // Correct way to access services
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  return useMutation<string, Error, Omit<MemoryItem, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'> & { media?: { mediaType: number, url: string }[], persons?: { memberId: string }[] }>({
    mutationFn: async (newMemoryItem) => {
      if (!newMemoryItem.familyId) {
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      const result = await services.memoryItem.createMemoryItem(newMemoryItem.familyId, newMemoryItem);
      if (result.ok) {
        return result.value;
      }
      showSnackbar(result.error?.message || t('memoryItem.messages.saveError'), 'error');
      throw result.error;
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['family', variables.familyId, 'memory-items'],
      });
      // Optionally, invalidate single item query if needed
      // queryClient.invalidateQueries({ queryKey: ['family', variables.familyId, 'memory-item', data] });
    },
  });
};
