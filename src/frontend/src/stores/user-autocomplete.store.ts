import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { User } from '@auth0/auth0-spa-js';

interface UserFilter {
  searchQuery?: string;
}

export const useUserAutocompleteStore = defineStore('userAutocomplete', {
  state: () => ({
    items: [] as User[],
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async search(filters: UserFilter): Promise<User[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.user.search(filters.searchQuery || '', 1, 50); // Use new IUserService

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

    async getByIds(ids: string[]): Promise<User[]> {
      const result = await this.services.user.getByIds(ids);
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
