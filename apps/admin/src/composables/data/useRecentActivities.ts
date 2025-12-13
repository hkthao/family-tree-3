import { useQuery } from '@tanstack/vue-query';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';
import type { ApiError, RecentActivity, TargetType } from '@/types';
import { computed, type Ref, type ComputedRef } from 'vue';



export function useRecentActivities(
  familyId: Ref<string | undefined> | ComputedRef<string | undefined>,
  page = 1,
  itemsPerPage = 10,
  targetType?: TargetType,
  targetId?: string,
) {
  const { user } = useServices();

  const query = useQuery<RecentActivity[] | undefined, ApiError>({
    queryKey: computed(() => queryKeys.userActivity.recent(familyId.value, page, itemsPerPage, targetType?.toString(), targetId)).value,
    queryFn: async () => {
      const result = await user.getRecentActivities(page, itemsPerPage, targetType, targetId, familyId.value);
      if (result.ok) {
        return result.value?.items || [];
      }
      throw result.error;
    },
    select: (data) => data,
    enabled: computed(() => !!user).value,
  });

  return {
    activities: query.data,
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    isFetching: query.isFetching,
    refetch: query.refetch,
  };
}
