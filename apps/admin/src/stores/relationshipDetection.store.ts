import { defineStore } from 'pinia';
import type { RelationshipDetectionResult } from '@/types';
import { useServices } from '@/composables';

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

        if (response && response.ok) {
          this.result = response.value;
          return response.value;
        } else if (response && !response.ok) {
          this.error = response.error?.message || 'Lỗi khi xác định quan hệ.';
          return null;
        } else { // response is null
          this.error = 'Không thể xác định quan hệ.';
          return null;
        }
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