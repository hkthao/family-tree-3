import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Family, FamilyFilter, Result } from '@/types';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';

export const useFamilyStore = defineStore('family', {
  state: () => ({
    error: null as string | null,

    list: {
      items: [] as Family[],
      loading: false,
      filter: {} as FamilyFilter,
      totalItems: 0,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[],
    },

    detail: {
      item: null as Family | null,
      loading: false,
    },

    add: {
      loading: false,
    },

    update: {
      loading: false,
    },

    _delete: {
      loading: false,
    },
  }),
  getters: {},
  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.family.loadItems(
        {
          ...this.list.filter,
          sortBy: this.list.sortBy.length > 0 ? this.list.sortBy[0].key : undefined,
          sortOrder:
            this.list.sortBy.length > 0
              ? (this.list.sortBy[0].order as 'asc' | 'desc')
              : undefined,
        },
        this.list.currentPage,
        this.list.itemsPerPage,
      );

      if (result.ok) {
        // Clear existing items and add new ones using splice to preserve array reference
        this.list.items.splice(0, this.list.items.length, ...result.value.items);
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('family.errors.load');
        this.list.items.splice(0, this.list.items.length); // Clear items on error
        this.list.totalItems = 0; // Reset totalItems on error
        this.list.totalPages = 1; // Reset totalPages on error
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.family.add(newItem);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('family.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async addItems(newItems: Omit<Family, 'id'>[]): Promise<Result<string[], ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.family.addItems(newItems);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('family.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: Family): Promise<void> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.family.update(updatedItem);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('family.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.family.delete(id);
      if (!result.ok) {
        this.error = i18n.global.t('family.errors.delete');
        console.error(result.error);
      } else {
        await this._loadItems();
      }
      this._delete.loading = false;
      return result;
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.list.totalPages && this.list.currentPage !== page) {
        this.list.currentPage = page;
        this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0 && this.list.itemsPerPage !== count) {
        this.list.itemsPerPage = count;
        this.list.currentPage = 1; // Reset to first page when items per page changes
        this._loadItems();
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.list.sortBy = sortBy;
      this.list.currentPage = 1; // Reset to first page on sort change
      this._loadItems();
    },

    setCurrentItem(item: Family | null) {
      this.detail.item = item;
    },

    async getById(id: string): Promise<Family | undefined> {
      this.detail.loading = true;
      this.error = null;

      // const cachedFamily = this.familyCache.get(id);
      // if (cachedFamily) {
      //   this.detail.loading = false;
      //   this.detail.item = cachedFamily;
      //   return cachedFamily;
      // }

      const result = await this.services.family.getById(id);
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

    async getByIds(ids: string[]): Promise<Family[]> {
      this.list.loading = true;
      this.error = null;

      const result = await this.services.family.getByIds(ids);
      this.list.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error =
          result.error.message || 'Không thể tải danh sách gia đình.';
        console.error(result.error);
        return [];
      }
    },
  },
})
