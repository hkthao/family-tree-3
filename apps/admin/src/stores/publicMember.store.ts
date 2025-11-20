import i18n from '@/plugins/i18n';
import type { Member } from '@/types';
import { defineStore } from 'pinia';

export const usePublicMemberStore = defineStore('publicMember', {
  state: () => ({
    error: null as string | null,
    list: {
      items: [] as Member[],
      loading: false,
    },
    detail: {
      item: null as Member | null,
      loading: false,
    },
  }),
  getters: {},
  actions: {
    async getPublicMembersByFamilyId(familyId: string): Promise<Member[]> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.publicMember.getPublicMembersByFamilyId(familyId);
      this.list.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = i18n.global.t('member.errors.load');
        console.error(result.error);
        return [];
      }
    },

    async getPublicMemberById(id: string, familyId: string): Promise<Member | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.publicMember.getPublicMemberById(id, familyId);
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
  },
});
