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
  }),

  getters: {
    getMembers: (state) => (familyId: string) => state.trees[familyId]?.members || [],
    getRelationships: (state) => (familyId: string) => state.trees[familyId]?.relationships || [],
    isLoading: (state) => (familyId: string) => state.loading[familyId] || false,
    getError: (state) => (familyId: string) => state.error[familyId] || null,
  },

  actions: {
    async fetchTreeData(familyId: string) {
      if (!familyId) return;

      this.loading[familyId] = true;
      this.error[familyId] = null;

      try {
        const [memberResult, relationshipResult] = await Promise.all([
          this.services.member.fetchMembersByFamilyId(familyId), // Use this.services.member
          this.services.relationship.loadItems({ familyId: familyId }, 1, 1000), // Use this.services.relationship
        ]);

        let fetchedMembers: Member[] = [];
        let fetchedRelationships: Relationship[] = [];

        if (memberResult.ok) {
          fetchedMembers = memberResult.value;
        } else {
          this.error[familyId] = memberResult.error;
        }

        if (relationshipResult.ok) {
          fetchedRelationships = relationshipResult.value.items;
        } else {
          this.error[familyId] = relationshipResult.error;
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
  },
});