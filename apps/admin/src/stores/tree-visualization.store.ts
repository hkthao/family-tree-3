import { defineStore } from 'pinia';
import type { Member, Relationship } from '@/types';

interface TreeData {
  members: Member[];
  relationships: Relationship[];
}

export const useTreeVisualizationStore = defineStore('tree-visualization', {
  state: () => ({
    trees: {} as Record<string, TreeData>,
    loading: {} as Record<string, boolean>,
    error: {} as Record<string, any>,
    searchQuery: '' as string, // Add searchQuery to state
  }),

  getters: {
    getMembers: (state) => (familyId: string) => state.trees[familyId]?.members || [],
    getRelationships: (state) => (familyId: string) => state.trees[familyId]?.relationships || [],
    isLoading: (state) => (familyId: string) => state.loading[familyId] || false,
    getError: (state) => (familyId: string) => state.error[familyId] || null,

    // New getters for filtered members and relationships
    getFilteredMembers: (state) => (familyId: string) => {
      const members = state.trees[familyId]?.members || [];
      if (!state.searchQuery) {
        return members;
      }
      const lowerCaseQuery = state.searchQuery.toLowerCase();
      return members.filter(member =>
        member.firstName.toLowerCase().includes(lowerCaseQuery) ||
        member.lastName.toLowerCase().includes(lowerCaseQuery)
      );
    },
    getFilteredRelationships: (state) => (familyId: string) => {
      const relationships = state.trees[familyId]?.relationships || [];
      const members = state.trees[familyId]?.members || [];
      if (!state.searchQuery) {
        return relationships;
      }
      const lowerCaseQuery = state.searchQuery.toLowerCase();
      const filteredMemberIds = new Set(
        members.filter(member =>
          member.firstName.toLowerCase().includes(lowerCaseQuery) ||
          member.lastName.toLowerCase().includes(lowerCaseQuery)
        ).map(m => m.id)
      );
      return relationships.filter(rel =>
        filteredMemberIds.has(rel.sourceMemberId) && filteredMemberIds.has(rel.targetMemberId)
      );
    },
  },

  actions: {
    async fetchTreeData(familyId: string, memberId?: string) {
      if (!familyId) return;

      this.loading[familyId] = true;
      this.error[familyId] = null;
      this.trees[familyId] = { members: [], relationships: [] };

      try {
        let fetchedMembers: Member[] = [];
        let fetchedRelationships: Relationship[] = [];

        if (memberId) {
          // Fetch data for a specific member and their direct relatives
          const memberResult = await this.services.member.getById(memberId);
          if (memberResult.ok && memberResult.value) {
            fetchedMembers.push(memberResult.value);
            // Assuming a service method to get relatives by memberId
            const relativesResult = await this.services.member.getRelatives(memberId);
            if (relativesResult.ok && relativesResult.value) {
              fetchedMembers = [...fetchedMembers, ...relativesResult.value];
            } else if (!relativesResult.ok) {
              this.error[familyId] = relativesResult.error;
            }
          } else if (!memberResult.ok) {
            this.error[familyId] = memberResult.error;
          }

          // Fetch relationships for the fetched members
          const memberIds = fetchedMembers.map(m => m.id);
          const relationshipResult = await this.services.relationship.search(
            {
              page: 1,
              itemsPerPage: 1000,
            },
            {
              familyId: familyId,
              memberIds: memberIds,
            }
          );
          if (relationshipResult.ok) {
            fetchedRelationships = relationshipResult.value.items;
          } else {
            this.error[familyId] = relationshipResult.error;
          }

        } else {
          // Original logic: Fetch all members and relationships for the family
          const [memberResult, relationshipResult] = await Promise.all([
            this.services.member.fetchMembersByFamilyId(familyId),
            this.services.relationship.getRelationShips(familyId),
          ]);

          if (memberResult.ok) {
            fetchedMembers = memberResult.value;
          } else {
            this.error[familyId] = memberResult.error;
          }

          if (relationshipResult.ok) {
            fetchedRelationships = relationshipResult.value;
          } else {
            this.error[familyId] = relationshipResult.error;
          }
        }

        this.trees[familyId] = {
          members: fetchedMembers,
          relationships: fetchedRelationships,
        };
      } catch (err) {
        this.error[familyId] = err;
      } finally {
        this.loading[familyId] = false;
      }
    },
    setSearchQuery(query: string) {
      this.searchQuery = query;
    },
  },
});