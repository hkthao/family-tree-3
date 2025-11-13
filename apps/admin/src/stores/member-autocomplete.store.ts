import { defineStore } from 'pinia';
import type { Member, MemberFilter } from '@/types';
import i18n from '@/plugins/i18n';

export const useMemberAutocompleteStore = defineStore('memberAutocomplete', {
  state: () => ({
    items: [] as Member[],
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async search(filters: MemberFilter): Promise<Member[]> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.member.loadItems(filters, 1, 50); // Assuming page 1, 50 items per page

        if (result.ok) {
          this.items = result.value.items;
          return result.value.items;
        } else {
          this.error = i18n.global.t('member.errors.load');
          console.error(result.error);
          this.items = [];
          return [];
        }
      } finally {
        this.loading = false;
      }
    },

    async getByIds(ids: string[]): Promise<Member[]> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.member.getByIds(ids);
        if (result.ok) {
          return result.value;
        } else {
          this.error = i18n.global.t('member.errors.loadById');
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