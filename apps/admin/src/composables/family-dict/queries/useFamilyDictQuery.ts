import { computed, unref, type Ref } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { FamilyDict } from '@/types';
import type { IFamilyDictService } from '@/services/family-dict/family-dict.service.interface';
import { queryKeys } from '@/constants/queryKeys';
import { useServices } from '@/composables';



export function useFamilyDictQuery(
  familyDictId: Ref<string | undefined>,
  service: IFamilyDictService = useServices().familyDict,
) {
  const query = useQuery<FamilyDict, Error>({
    queryKey: computed(() => queryKeys.familyDicts.detail(unref(familyDictId) as string)),
    queryFn: async () => {
      const id = unref(familyDictId);
      if (!id) {
        throw new Error('FamilyDict ID is required');
      }
      const response = await service.getById(id);
      if (response.ok) {
        if (response.value === undefined) {
          throw new Error('FamilyDict not found');
        }
        return response.value;
      }
      throw response.error;
    },
    enabled: computed(() => !!unref(familyDictId)), // Only run query if familyDictId is available
    staleTime: 1000 * 60 * 1, // 1 minute
  });

  const familyDict = computed(() => query.data.value);
  const loading = computed(() => query.isFetching.value);

  return {
    query,
    familyDict,
    loading,
    error: query.error,
    refetch: query.refetch,
  };
}