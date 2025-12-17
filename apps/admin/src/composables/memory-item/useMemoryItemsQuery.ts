import { type Ref, computed } from 'vue';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { Paginated, ListOptions, MemoryItem } from '@/types';
import type { MemoryItemFilter } from '@/services/memory-item/memory-item.service.interface';
import { useServices } from '@/plugins/services.plugin';

export const useMemoryItemsQuery = (
  familyId: Ref<string | undefined>,
  options: Ref<ListOptions>,
  filters: Ref<MemoryItemFilter>,
) => {
  const services = useServices();
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  const combinedFilters = computed(() => ({ ...filters.value, familyId: familyId.value }));

  const query = useQuery<Paginated<MemoryItem>, Error>({
    queryKey: ['memory-items', options, combinedFilters],
    queryFn: async () => {
      if (!familyId.value) { // Still need familyId for the filter to be valid
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      const result = await services.memoryItem.search(
        options.value,
        combinedFilters.value,
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
    enabled: computed(() => !!familyId.value),
    placeholderData: (previousData: Paginated<MemoryItem> | undefined) => previousData,
  });

  const invalidateMemoryItemsQuery = () => {
    queryClient.invalidateQueries({
      queryKey: ['memory-items', { familyId: familyId.value }], // Invalidate based on the filter
    });
  };

  return {
    ...query,
    invalidateMemoryItemsQuery,
  };
};
