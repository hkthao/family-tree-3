import { useQuery, type UseQueryOptions, type UseQueryReturnType } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError, DashboardStats } from '@/types';
import { computed, type Ref, type ComputedRef } from 'vue';
import type { IDashboardService } from '@/services/dashboard/dashboard.service.interface';

interface UseDashboardStatsDeps {
  useQuery: <TData = unknown, TError = Error>(
    options: UseQueryOptions<TData, TError>,
  ) => UseQueryReturnType<TData, TError>;
  getDashboardService: () => IDashboardService;
}

const defaultDeps: UseDashboardStatsDeps = {
  useQuery,
  getDashboardService: () => useServices().dashboard,
};

export function useDashboardStats(
  familyId: Ref<string | undefined> | ComputedRef<string | undefined>,
  deps: UseDashboardStatsDeps = defaultDeps,
) {
  const { useQuery } = deps;
  const dashboardService = deps.getDashboardService(); // Get the service from the function

  const query = useQuery<DashboardStats | undefined, ApiError>({
    queryKey: computed(() => queryKeys.dashboard.stats(familyId.value)),
    queryFn: async () => {
      const result = await dashboardService.fetchStats(familyId.value);
      if (result.ok) {
        return result.value;
      }
      throw result.error;
    },
    select: (data) => data,
    enabled: true, // Temporarily set to true for debugging
  });

  return {
    state: {
      dashboardStats: query.data,
      isLoading: query.isLoading,
      isError: query.isError,
      error: query.error,
      isFetching: query.isFetching,
    },
    actions: {
      refetch: query.refetch,
    },
  };
}
