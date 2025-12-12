import { computed, type ComputedRef } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { MemberFace, Paginated, ListOptions, FilterOptions } from '@/types';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';

export const useMemberFacesQuery = (options: ComputedRef<ListOptions>, filters: ComputedRef<FilterOptions>) => {
  const services = useServices();

  const query = useQuery({
    queryKey: computed(() => queryKeys.memberFaces.list(options.value, filters.value)),
    queryFn: async () => {
      const response = await services.memberFace.search(options.value, filters.value);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to fetch member faces');
    },
    // Keep data in cache for 5 minutes
    staleTime: 1000 * 60 * 5,
  });

  const memberFaces = computed<MemberFace[]>(() => query.data.value?.items || []);
  const totalItems = computed<number>(() => query.data.value?.totalItems || 0);
  const queryLoading = computed<boolean>(() => query.isPending.value || query.isFetching.value);
  const queryError = computed<Error | null>(() => query.error.value);

  return {
    memberFaces,
    totalItems,
    queryLoading,
    queryError,
    ...query, // Expose all query properties
  };
};
