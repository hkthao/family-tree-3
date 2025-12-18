import { computed, ref, watch, unref, type MaybeRef } from 'vue';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useI18n } from 'vue-i18n';
import { useServices } from '@/composables';
import { queryKeys } from '@/constants/queryKeys';
import type { Member, Relationship } from '@/types';

export function useTreeVisualization(familyId: MaybeRef<string | undefined>, initialMemberId?: MaybeRef<string | undefined>) {
  const { t } = useI18n();
  const { member, relationship } = useServices();
  const queryClient = useQueryClient();

  const selectedRootMemberId = ref<string | undefined>(unref(initialMemberId));

  // Fetch members
  const { data: members, isLoading: isLoadingMembers, isError: isErrorMembers, error: errorMembers } = useQuery<Member[], Error>({
    queryKey: queryKeys.families.membersByFamilyId(unref(familyId) || ''),
    queryFn: async () => {
      const currentFamilyId = unref(familyId);
      if (!currentFamilyId) return Promise.reject(new Error(t('family.tree.familyIdRequired')));
      const result = await member.fetchMembersByFamilyId(currentFamilyId);
      if (result.ok) {
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!unref(familyId)),
  });

  // Fetch relationships
  const { data: relationships, isLoading: isLoadingRelationships, isError: isErrorRelationships, error: errorRelationships } = useQuery<Relationship[], Error>({
    queryKey: queryKeys.families.relationshipsByFamilyId(unref(familyId) || ''),
    queryFn: async () => {
      const currentFamilyId = unref(familyId);
      if (!currentFamilyId) return Promise.reject(new Error(t('family.tree.familyIdRequired')));
      const result = await relationship.getRelationShips(currentFamilyId);
      if (result.ok) {
        return result.value;
      } else {
        throw result.error;
      }
    },
    enabled: computed(() => !!unref(familyId)),
  });

  const isLoading = computed(() => isLoadingMembers.value || isLoadingRelationships.value);
  const isError = computed(() => isErrorMembers.value || isErrorRelationships.value);
  const error = computed(() => errorMembers.value || errorRelationships.value);

  const allMembers = computed(() => members.value || []);
  const allRelationships = computed(() => relationships.value || []);

  const getFilteredMembers = computed(() => {
    if (!selectedRootMemberId.value) {
      return allMembers.value;
    }
    // Simple filtering for now - might need more complex tree traversal later
    // For now, assume if a member is in the relationships, they are part of the tree from the root
    const membersInRelationships = new Set<string>();
    allRelationships.value.forEach(rel => {
      membersInRelationships.add(rel.sourceMemberId);
      membersInRelationships.add(rel.targetMemberId);
    });

    return allMembers.value.filter(m => m.id === selectedRootMemberId.value || membersInRelationships.has(m.id));
  });

  const getFilteredRelationships = computed(() => {
    if (!selectedRootMemberId.value) {
      return allRelationships.value;
    }
    // Filter relationships where either member is the root or part of the filtered members
    const filteredMemberIds = new Set(getFilteredMembers.value.map(m => m.id));
    return allRelationships.value.filter(rel =>
      filteredMemberIds.has(rel.sourceMemberId) && filteredMemberIds.has(rel.targetMemberId)
    );
  });

  const fetchTreeData = async (newFamilyId: MaybeRef<string | undefined>, newInitialMemberId?: MaybeRef<string | undefined>) => {
    const unrefNewFamilyId = unref(newFamilyId);
    if (unrefNewFamilyId) {
      queryClient.invalidateQueries({ queryKey: queryKeys.families.membersByFamilyId(unrefNewFamilyId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.families.relationshipsByFamilyId(unrefNewFamilyId) });
      selectedRootMemberId.value = unref(newInitialMemberId);
    }
  };

  watch(() => unref(initialMemberId), (newInitialMemberId) => {
    selectedRootMemberId.value = newInitialMemberId;
  });

  return {
    members: getFilteredMembers,
    relationships: getFilteredRelationships,
    isLoading,
    isError,
    error,
    selectedRootMemberId,
    fetchTreeData, // Expose a method to re-fetch/invalidate queries
  };
}
