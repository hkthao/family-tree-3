import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { UserProfile, Result } from '@/types';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';
import { serviceFactory } from '@/services/service.factory';
import type { ICurrentUserProfileService } from '@/services/user-profile/user-profile.service.interface';

interface UserFilter {
  searchQuery?: string;
}

export const useUserStore = defineStore('user', {
  state: () => ({
    items: [] as UserProfile[],
    currentItem: null as UserProfile | null,
    loading: false,
    error: null as string | null,
    filter: {} as UserFilter,
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    totalPages: 1,
    sortBy: [] as { key: string; order: string }[],
  }),
  getters: {},
  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const userProfileService = serviceFactory.get('IUserProfileService') as ICurrentUserProfileService;
      const result = await userProfileService.getAllUserProfiles(); // Assuming getAllUserProfiles handles filtering/pagination internally or we'll add it later

      if (result.ok) {
        // For now, simple filtering based on searchQuery
        const filteredItems = result.data.filter(user =>
          user.name.toLowerCase().includes(this.filter.searchQuery?.toLowerCase() || '')
        );
        this.items.splice(0, this.items.length, ...filteredItems);
        this.totalItems = filteredItems.length;
        this.totalPages = Math.ceil(this.totalItems / this.itemsPerPage);
      } else {
        this.error = i18n.global.t('common.errors.loadUsers'); // Need to add this i18n key
        this.items.splice(0, this.items.length); 
        this.totalItems = 0; 
        this.totalPages = 1; 
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<UserProfile, 'id'>): Promise<Result<UserProfile, ApiError>> {
      // User creation is typically handled by authentication flow, not directly via a store action.
      // This action might not be needed or would delegate to an identity service.
      // For now, returning a dummy error.
      return { ok: false, error: { message: "User creation not supported via this store action." } as ApiError };
    },

    async updateItem(updatedItem: UserProfile): Promise<Result<UserProfile, ApiError>> {
      this.loading = true;
      this.error = null;
      const userProfileService = serviceFactory.get('IUserProfileService') as ICurrentUserProfileService;
      const result = await userProfileService.updateUserProfile(updatedItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('common.errors.updateUser'); // Need to add this i18n key
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      // User deletion is typically handled by admin features, not directly via a store action.
      // For now, returning a dummy error.
      return { ok: false, error: { message: "User deletion not supported via this store action." } as ApiError };
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages && this.currentPage !== page) {
        this.currentPage = page;
        this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0 && this.itemsPerPage !== count) {
        this.itemsPerPage = count;
        this.currentPage = 1;
        this._loadItems();
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.sortBy = sortBy;
      this.currentPage = 1;
      this._loadItems();
    },

    setCurrentItem(item: UserProfile | null) {
      this.currentItem = item;
    },

    async getById(id: string): Promise<UserProfile | undefined> {
      this.loading = true;
      this.error = null;
      const userProfileService = serviceFactory.get('IUserProfileService') as ICurrentUserProfileService;
      const result = await userProfileService.getUserProfile(id);
      this.loading = false;
      if (result.ok) {
        if (result.value) {
          this.currentItem = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('common.errors.loadUserById'); // Need to add this i18n key
        console.error(result.error);
      }
      return undefined;
    },

    async getByIds(ids: string[]): Promise<UserProfile[]> {
      this.loading = true;
      this.error = null;
      const userProfileService = serviceFactory.get('IUserProfileService') as ICurrentUserProfileService;
      const preloadedUsers: UserProfile[] = [];
      for (const id of ids) {
        const result = await userProfileService.getUserProfile(id);
        if (result.ok) {
          preloadedUsers.push(result.data);
        }
      }
      this.loading = false;
      return preloadedUsers;
    },
  },
});
