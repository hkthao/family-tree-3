import { computed, ref, watch, unref, type MaybeRef } from 'vue';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/plugins/services.plugin';
import { queryKeys } from '@/constants/queryKeys';
import type { MemberDto, Relationship, IFamilyTreeData } from '@/types';

export function useTreeVisualization(familyId: MaybeRef<string | undefined>, initialMemberId?: MaybeRef<string | undefined>) {
  const { t } = useI18n();
  const { family } = useServices();
  const queryClient = useQueryClient();

  const selectedRootMemberId = ref<string | undefined>(unref(initialMemberId));

  // Accumulated data
  const allMembers = ref<MemberDto[]>([]);
  const allRelationships = ref<Relationship[]>([]);

  // Computed query key to ensure reactivity
  const computedQueryKey = computed(() => queryKeys.families.treeData(
    unref(familyId) || '',
    selectedRootMemberId.value,
  ));

  const { isLoading, isError, error } = useQuery<IFamilyTreeData, Error>({
    queryKey: computedQueryKey,
    queryFn: async () => {
      const currentFamilyId = unref(familyId);
      if (!currentFamilyId) return Promise.reject(new Error(t('family.tree.familyIdRequired')));

      const result = await family.fetchFamilyTreeData(currentFamilyId, selectedRootMemberId.value ?? null);
      if (result.ok) {
        allMembers.value = result.value.members;
        allRelationships.value = result.value.relationships;
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!unref(familyId)), // Only fetch if familyId exists
  });

  const fetchTreeData = async (newFamilyId: MaybeRef<string | undefined>, newInitialMemberId?: MaybeRef<string | undefined>) => {
    const unrefNewFamilyId = unref(newFamilyId);
    const unrefNewInitialMemberId = unref(newInitialMemberId);

    if (unrefNewFamilyId) {
      // Reset state for new fetch
      allMembers.value = [];
      allRelationships.value = [];
      selectedRootMemberId.value = unrefNewInitialMemberId;

      // Invalidate queries to refetch from scratch with new parameters
      queryClient.invalidateQueries({
        queryKey: queryKeys.families.treeData(unrefNewFamilyId, unrefNewInitialMemberId)
      });
    }
  };

  watch(() => unref(initialMemberId), (newInitialMemberId) => {
    if (unref(familyId)) {
      fetchTreeData(unref(familyId), newInitialMemberId);
    }
  });

  watch(() => unref(familyId), (newFamilyId) => {
    if (newFamilyId) {
      fetchTreeData(newFamilyId, unref(initialMemberId));
    }
  });

  return {
    state: {
      members: allMembers,
      relationships: allRelationships,
      isLoading,
      isError,
      error,
      selectedRootMemberId,
    },
    actions: {
      fetchTreeData,
    },
  };
}
