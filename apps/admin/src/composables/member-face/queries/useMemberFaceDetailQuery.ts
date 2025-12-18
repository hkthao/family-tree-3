import { computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { MemberFace } from '@/types';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/plugins/services.plugin';

export const useMemberFaceDetailQuery = (id: string) => {
  const services = useServices();

  const query = useQuery({
    queryKey: computed(() => queryKeys.memberFaces.detail(id)),
    queryFn: async () => {
      if (!id) {
        return undefined;
      }
      const response = await services.memberFace.getById(id);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to fetch member face detail');
    },
    enabled: computed(() => !!id), // Only run query if id is provided
    staleTime: 1000 * 60 * 5, // Keep data in cache for 5 minutes
  });

  const memberFace = computed<MemberFace | undefined>(() => query.data.value);
  const queryLoading = computed<boolean>(() => query.isPending.value || query.isFetching.value);
  const queryError = computed<Error | null>(() => query.error.value);

  return {
    memberFace,
    queryLoading,
    queryError,
    ...query, // Expose all query properties
  };
};
