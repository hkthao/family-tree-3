import i18n from '@/plugins/i18n';
import type { Relationship } from '@/types';
import { defineStore } from 'pinia';

export const usePublicRelationshipStore = defineStore('publicRelationship', {
  state: () => ({
    error: null as string | null,
    list: {
      items: [] as Relationship[],
      loading: false,
    },
  }),
  getters: {},
  actions: {
    async getPublicRelationshipsByFamilyId(familyId: string): Promise<Relationship[]> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.publicRelationship.getPublicRelationshipsByFamilyId(familyId);
      this.list.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = i18n.global.t('relationship.errors.load'); // Assuming a translation key for relationship errors
        console.error(result.error);
        return [];
      }
    },
  },
});
