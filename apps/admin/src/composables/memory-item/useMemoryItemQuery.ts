import { type Ref, computed } from 'vue'; // Added computed
import { useQuery } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { MemoryItem } from '@/types';
import { useServices } from '@/plugins/services.plugin'; // Correct import

export const useMemoryItemQuery = (
  familyId: Ref<string | undefined>,
  memoryItemId: Ref<string | undefined>,
) => {
  const services = useServices(); // Correct way to access services
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();

  const query = useQuery<MemoryItem, Error>({
    queryKey: ['family', familyId, 'memory-item', memoryItemId],
    queryFn: async () => {
      if (!familyId.value) {
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      if (!memoryItemId.value) {
        throw new Error(t('memoryItem.messages.noMemoryItemId'));
      }
      const result = await services.memoryItem.getMemoryItemById(
        familyId.value,
        memoryItemId.value,
      );
      if (result.ok) {
        return result.value;
      }
      showSnackbar(
        result.error?.message || t('memoryItem.messages.loadError'),
        'error',
      );
      throw result.error;
    },
    enabled: computed(() => !!familyId.value && !!memoryItemId.value),
  });

  return query;
};
