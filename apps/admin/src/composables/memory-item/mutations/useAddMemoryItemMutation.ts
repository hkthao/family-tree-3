import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { MemoryItem, AddMemoryItemDto } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useAddMemoryItemMutation = () => {
  const services = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  return useMutation<MemoryItem, Error, AddMemoryItemDto>({
    mutationFn: async (newMemoryItem: AddMemoryItemDto) => {
      if (!newMemoryItem.familyId) {
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      const result = await services.memoryItem.add(newMemoryItem);
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
    },
  });
};
