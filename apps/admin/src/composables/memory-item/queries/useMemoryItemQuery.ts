import { type Ref, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { MemoryItem } from '@/types';
import { useServices } from '@/plugins/services.plugin';

export const useMemoryItemQuery = (
  familyIdParam: Ref<string | undefined> | string,
  memoryItemIdParam: Ref<string | undefined> | string,
) => {
  const services = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();

  // Convert params to refs if they are not already
  // familyId is no longer used internally, so no need to create a computed ref for it
  const memoryItemId = computed(() => (typeof memoryItemIdParam === 'string' ? memoryItemIdParam : memoryItemIdParam.value));

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
