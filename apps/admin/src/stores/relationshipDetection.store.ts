import { defineStore } from 'pinia';
import type { RelationshipDetectionResult } from '@/types';
import { useServices } from '@/composables/useServices'; // Import useServices

interface RelationshipDetectionState {
  result: RelationshipDetectionResult | null;
  loading: boolean;
  error: string | null;
}

export const useRelationshipDetectionStore = defineStore('relationshipDetection', {
  state: (): RelationshipDetectionState => ({
    result: null,
    loading: false,
    error: null,
  }),
  actions: {
    async detectRelationship(familyId: string, memberAId: string, memberBId: string): Promise<RelationshipDetectionResult | null> {
      this.loading = true;
      this.error = null;
      this.result = null;
      try {
        const { relationship } = useServices(); // Get the relationship service
        const response = await relationship.detectRelationship(familyId, memberAId, memberBId);
        this.result = response;
        return response;
      } catch (err: any) {
        this.error = err.message || 'Lỗi khi xác định quan hệ.';
        return null;
      } finally {
        this.loading = false;
      }
    },
    clearResult() {
      this.result = null;
      this.error = null;
    }
  },
});