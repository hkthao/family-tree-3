import i18n from '@/plugins/i18n';
import type { Member } from '@/types';
import { defineStore } from 'pinia';

export const useFaceMemberSelectStore = defineStore('faceMemberSelect', {
  state: () => ({
    detail: {
      item: null as Member | null,
      loading: false,
    },
    error: null as string | null,
  }),

  actions: {
    async getById(id: string): Promise<Member | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.member.getById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('member.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },
    setCurrentItem(item: Member | null) {
      this.detail.item = item;
    },
  },
});
