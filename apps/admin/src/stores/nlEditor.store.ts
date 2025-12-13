import i18n from '@/plugins/i18n';
import type { Member } from '@/types';
import { defineStore } from 'pinia';

export const useNLEditorStore = defineStore('nlEditor', {
  state: () => ({
    list: {
      items: [] as Member[],
      loading: false,
    },
    error: null as string | null,
  }),

  actions: {
    async searchMembers(searchQuery: string) {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.search(
        {
          page: 1,
          itemsPerPage: 10,
        },
        {
          searchQuery,
        }
      );

      if (result.ok) {
        this.list.items = result.value.items;
      } else {
        this.error = i18n.global.t('member.errors.load');
        this.list.items = [];
        console.error(result.error);
      }
      this.list.loading = false;
    },
  },
});
