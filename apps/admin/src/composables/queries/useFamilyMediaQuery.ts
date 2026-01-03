// apps/admin/src/composables/queries/useFamilyMediaQuery.ts
import { computed, type ComputedRef } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyMedia, FamilyMediaFilter, Paginated, ListOptions } from '@/types';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';

export const useFamilyMediaQuery = (
  options: ComputedRef<ListOptions>,
  filters: ComputedRef<FamilyMediaFilter>,
) => {
  const services = useServices();

  const query = useQuery({
    queryKey: computed(() => queryKeys.familyMedia.list(options.value, filters.value)),
    queryFn: async (): Promise<Paginated<FamilyMedia>> => {
      const response = await services.familyMedia.search(options.value, filters.value);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to fetch family media');
    },
    staleTime: 1000 * 60 * 5, // Keep data in cache for 5 minutes
  });

  const familyMedia = computed<FamilyMedia[]>(() => query.data.value?.items || []);
  const totalItems = computed<number>(() => query.data.value?.totalItems || 0);
  const queryLoading = computed<boolean>(() => query.isPending.value || query.isFetching.value);
  const queryError = computed<Error | null>(() => query.error.value);

  return {
    familyMedia,
    totalItems,
    queryLoading,
    queryError,
    ...query, // Expose all query properties
  };
};
