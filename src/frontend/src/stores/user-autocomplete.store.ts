import { defineStore } from 'pinia';
import type { UserProfile, Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import i18n from '@/plugins/i18n';

interface UserFilter {
  searchQuery?: string;
}

export const useUserAutocompleteStore = defineStore('userAutocomplete', {
  state: () => ({
    items: [] as UserProfile[],
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async search(filters: UserFilter): Promise<UserProfile[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.user.searchUsers(filters.searchQuery || '', 1, 50); // Use new IUserService

      if (result.ok) {
        this.items = result.value.items;
        return result.value.items;
      } else {
        this.error = i18n.global.t('common.errors.loadUsers');
        console.error(result.error);
        this.items = [];
        return [];
      }
    },

    async getByIds(ids: string[]): Promise<UserProfile[]> {
      const result = await this.services.user.getUsersByIds(ids);
      if (result.ok) {
        return result.value;
      } else {
        console.error(result.error);
        return [];
      }
    },

    clearItems() {
      this.items = [];
    },
  },
});
