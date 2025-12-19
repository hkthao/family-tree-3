import { computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/composables';
import { queryKeys } from '@/constants/queryKeys';
import type { Relationship } from '@/types';

export function useMemberRelationships(memberId: string, familyId: string) {
  const { t } = useI18n();
  const { relationship } = useServices();

  // Fetch all relationships for the family
  const {
    data: allRelationships,
    isLoading,
    isError,
    error,
  } = useQuery<Relationship[], Error>({
    queryKey: queryKeys.families.relationshipsByFamilyId(familyId), // Fetch by familyId
    queryFn: async () => {
      if (!familyId) return Promise.reject(new Error(t('relationship.familyIdRequired')));
      const result = await relationship.getRelationShips(familyId);
      if (result.ok) {
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!familyId),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const filteredRelationships = computed(() => {
    if (!memberId || !allRelationships.value) return [];
    return allRelationships.value.filter(
      (rel: Relationship) =>
        rel.sourceMemberId === memberId || rel.targetMemberId === memberId
    );
  });

  return {
    relationships: filteredRelationships,
    isLoading,
    isError,
    error,
  };
}
