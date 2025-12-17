import { type Ref, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { MemoryItem } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useMemoryItemQuery = (
  familyId: Ref<string | undefined>, // Keep familyId in signature for context if needed elsewhere
  memoryItemId: Ref<string | undefined>,
) => {
  const services = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();

  const query = useQuery<MemoryItem, Error>({
    queryKey: ['memory-items', memoryItemId],
    queryFn: async () => {
      if (!memoryItemId.value) {
        throw new Error(t('memoryItem.messages.noMemoryItemId'));
      }
      const result = await services.memoryItem.getById(memoryItemId.value);
      if (result.ok) {
        if (!result.value) {
          throw new Error(t('common.messages.notFound', { item: t('memoryItem.title') }));
        }
        return result.value;
      }
      showSnackbar(
        result.error?.message || t('memoryItem.messages.loadError'),
        'error',
      );
      throw result.error;
    },
    enabled: computed(() => !!memoryItemId.value),
  });

  return query;
};
