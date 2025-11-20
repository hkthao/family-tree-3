import i18n from '@/plugins/i18n';
import type { Family } from '@/types';
import { defineStore } from 'pinia';

export const usePublicFamilyStore = defineStore('publicFamily', {
  state: () => ({
    error: null as string | null,
    detail: {
      item: null as Family | null,
      loading: false,
    },
  }),
  getters: {},
  actions: {
    async getPublicFamilyById(id: string): Promise<Family | undefined> {
      this.detail.loading = true;
      this.error = null;

      const result = await this.services.publicFamily.getPublicFamilyById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('family.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },
  },
});
