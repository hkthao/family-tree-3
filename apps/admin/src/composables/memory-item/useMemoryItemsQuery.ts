import { type Ref, computed } from 'vue'; // Added computed
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { Paginated, ListOptions, MemoryItem } from '@/types'; // Changed PaginatedList to Paginated
import type { MemoryItemFilter } from '@/services/memory-item/memory-item.service.interface';
import { useServices } from '@/plugins/services.plugin'; // Correct import

export const useMemoryItemsQuery = (
  familyId: Ref<string | undefined>,
  options: Ref<ListOptions>,
  filters: Ref<MemoryItemFilter>,
) => {
  const services = useServices(); // Correct way to access services
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const queryClient = useQueryClient();

  const query = useQuery<Paginated<MemoryItem>, Error>({ // Changed PaginatedList to Paginated
    queryKey: ['family', familyId, 'memory-items', options, filters],
    queryFn: async () => {
      if (!familyId.value) {
        throw new Error(t('memoryItem.messages.noFamilyId'));
      }
      const result = await services.memoryItem.searchMemoryItems(
        familyId.value,
        options.value,
        filters.value,
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
    placeholderData: (previousData: Paginated<MemoryItem> | undefined) => previousData, // Explicitly type previousData
  });

  const invalidateMemoryItemsQuery = () => {
    queryClient.invalidateQueries({
      queryKey: ['family', familyId, 'memory-items'],
    });
  };

  return {
    ...query,
    invalidateMemoryItemsQuery,
  };
};
