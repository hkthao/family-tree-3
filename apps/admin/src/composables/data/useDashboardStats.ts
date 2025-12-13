import { useQuery } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError, DashboardStats } from '@/types';
import { computed, type Ref, type ComputedRef } from 'vue';

export function useDashboardStats(familyId: Ref<string | undefined> | ComputedRef<string | undefined>) {
  const { dashboard } = useServices();

  const query = useQuery<DashboardStats | undefined, ApiError>({
    queryKey: computed(() => queryKeys.dashboard.stats(familyId.value)).value,
    queryFn: async () => {
      const result = await dashboard.fetchStats(familyId.value);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    select: (data) => data,
    enabled: computed(() => !!dashboard).value, // Ensure service is available
  });

  return {
    dashboardStats: query.data,
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    isFetching: query.isFetching,
    refetch: query.refetch,
  };
}
