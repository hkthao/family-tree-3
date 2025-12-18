import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Family } from '@/types';
import { ApiFamilyService } from '@/services/family/api.family.service';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { apiClient } from '@/plugins/axios';
import { queryKeys } from '@/constants/queryKeys';

const apiFamilyService: IFamilyService = new ApiFamilyService(apiClient);

export function useFamilyQuery(familyId: Ref<string | undefined>) {
  const query = useQuery<Family, Error>({
    queryKey: computed(() => (unref(familyId) ? queryKeys.families.detail(unref(familyId)!) : [])),
    queryFn: async () => {
      const id = unref(familyId);
      if (!id) {
        throw new Error('Family ID is required');
      }
      const response = await apiFamilyService.getById(id);
      if (response.ok) {
        if (response.value === undefined) {
          throw new Error('Family not found');
        }
        return response.value;
      }
      throw response.error;
    },
    enabled: computed(() => !!unref(familyId)),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const family = computed(() => query.data.value);
  const isLoading = computed(() => query.isFetching.value);

  return {
    query,
    family,
    isLoading,
    error: query.error,
    refetch: query.refetch,
  };
}