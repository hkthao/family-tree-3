import { defineStore } from 'pinia';
import type { Family, FamilyFilter } from '@/types';
import i18n from '@/plugins/i18n';

export const useFamilyAutocompleteStore = defineStore('familyAutocomplete', {
  state: () => ({
    items: [] as Family[],
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async search(filters: FamilyFilter): Promise<Family[]> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.family.loadItems(filters, 1, 50); // Assuming page 1, 50 items per page

        if (result.ok) {
          this.items = result.value.items;
          return result.value.items;
        } else {
          this.error = i18n.global.t('family.errors.load');
          console.error(result.error);
          this.items = [];
          return [];
        }
      } finally {
        this.loading = false;
      }
    },

    async getByIds(ids: string[]): Promise<Family[]> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.family.getByIds(ids);
        if (result.ok) {
          return result.value;
        } else {
          this.error = i18n.global.t('family.errors.loadById');
          console.error(result.error);
          return [];
        }
      } finally {
        this.loading = false;
      }
    },

    clearItems() {
      this.items = [];
    },
  },
});
