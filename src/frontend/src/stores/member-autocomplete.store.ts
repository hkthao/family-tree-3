import { defineStore } from 'pinia';
import type { Member, MemberFilter } from '@/types/member';
import { IdCache } from '@/utils/cacheUtils';
import i18n from '@/plugins/i18n';
import type { ApiError } from '@/plugins/axios';

export const useMemberAutocompleteStore = defineStore('memberAutocomplete', {
  state: () => ({
    members: [] as Member[],
    loading: false,
    error: null as string | null,
    memberCache: new IdCache<Member>(),
  }),
  getters: {
    items: (state) => state.members,
  },
  actions: {
    async searchMembers(filter: MemberFilter, page: number = 1, itemsPerPage: number = 20) {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.loadItems(
        {
          ...filter,
        },
        page,
        itemsPerPage,
      );

      if (result.ok && result.value) {
        this.members.splice(0, this.members.length, ...result.value.items);
        this.memberCache.setMany(result.value.items);
      } else {
        this.error = (result as { ok: false; error: ApiError }).error?.message || i18n.global.t('member.errors.load');
        this.members.splice(0, this.members.length); // Clear items on error
      }
      this.loading = false;
    },

    async getMemberByIds(ids: string[]): Promise<Member[]> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.memberCache.getMany(ids, (missingIds) =>
          this.services.member.getByIds(missingIds),
        );

        if (result.ok && result.value) {
          return result.value;
        } else {
          this.error = (result as { ok: false; error: ApiError }).error?.message || i18n.global.t('member.errors.loadById');
          return [];
        }
      } catch (error: any) {
        this.error = error.message || i18n.global.t('member.errors.loadById');
        console.error('Error fetching members by IDs:', error);
        return [];
      } finally {
        this.loading = false;
      }
    },
  },
});