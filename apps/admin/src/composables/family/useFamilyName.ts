import { computed, type Ref } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import type { IFamilyService } from '@/services/family/family.service.interface';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { FamilyDto } from '@/types';
import { useQuery } from '@tanstack/vue-query';

const FAMILY_QUERY_KEY = 'family';

interface UseFamilyNameDeps {
  familyService: IFamilyService;
}

export function useFamilyName(
  familyId: Ref<string | null>,
  deps?: Partial<UseFamilyNameDeps>,
) {
  const services = useServices();
  const familyService = deps?.familyService || services.family;

  const queryKey = computed(() => [FAMILY_QUERY_KEY, familyId.value]);

  const { data: family, isLoading } = useQuery({
    queryKey,
    queryFn: async () => {
      if (!familyId.value) {
        return null;
      }
      const result = await familyService.getById(familyId.value);
      return result.ok ? result.value : null;
    },
    enabled: computed(() => !!familyId.value),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const familyName = computed(() => family.value?.name || null);

  return {
    state: {
      familyName,
      isLoading,
    },
  };
}