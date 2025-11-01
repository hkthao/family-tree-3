import { defineStore } from 'pinia';

import type { Family, FamilyFilter } from '@/types/family';
import i18n from '@/plugins/i18n';
import type { ApiError } from '@/plugins/axios';



export const useFamilyAutocompleteStore = defineStore('familyAutocomplete', {
  state: () => ({
    families: [] as Family[],
    loading: false,
    error: null as string | null,
  }),
  getters: {
    items: (state) => state.families,
  },
  actions: {
    async searchFamilies(filter: FamilyFilter, page: number = 1, itemsPerPage: number = 20) {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.loadItems(
        {
          ...filter,
        },
        page,
        itemsPerPage,
      );

      if (result.ok && result.value) {
        this.families.splice(0, this.families.length, ...result.value.items);
      } else {
        this.error = (result as { ok: false; error: ApiError }).error?.message || i18n.global.t('family.errors.load');
        this.families.splice(0, this.families.length); // Clear items on error
      }
      this.loading = false;
    },

    async getFamilyByIds(ids: string[]): Promise<Family[]> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.family.getByIds(ids);
        if (result.ok && result.value) {
          return result.value;
        } else {
          this.error = (result as { ok: false; error: ApiError }).error?.message || i18n.global.t('family.errors.loadById');
          return [];
        }
      } catch (error: any) {
        this.error = error.message || i18n.global.t('family.errors.loadById');
        console.error('Error fetching families by IDs:', error);
        return [];
      } finally {
        this.loading = false;
      }
    },
  },
});