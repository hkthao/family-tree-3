import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Family, FamilyFilter } from '@/types';
import { defineStore } from 'pinia';

export const useFamilyStore = defineStore('family', {
  state: () => ({
    items: [] as Family[],
    currentItem: null as Family | null,
    loading: false,
    error: null as string | null,
    filter: {} as FamilyFilter, // New filter object
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE, // Default items per page
    totalPages: 1,
    sortBy: [] as { key: string; order: string }[], // Sorting key and order
  }),
  getters: {},
  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.loadItems(
        this.filter,
        this.currentPage,
        this.itemsPerPage,
        this.sortBy.length > 0 ? this.sortBy[0].key : undefined,
        this.sortBy.length > 0 ? (this.sortBy[0].order as 'asc' | 'desc') : undefined,
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('family.errors.load');
        this.items = []; // Clear items on error
        this.totalItems = 0; // Reset totalItems on error
        this.totalPages = 1; // Reset totalPages on error
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<Family, 'id'>) {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.add(newItem);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('family.errors.add');
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: Family) {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.update(updatedItem);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('family.errors.update');
        console.error(result.error);
      }
      this.loading = false;
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.delete(id);
      if (!result.ok) {
        this.error = i18n.global.t('family.errors.delete');
        console.error(result.error);
      } else {
        await this._loadItems();
      }
      this.loading = false;
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
        this.currentPage = 1; // Reset to first page when items per page changes
        this._loadItems();
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.sortBy = sortBy;
      this.currentPage = 1; // Reset to first page on sort change
      this._loadItems();
    },

    setCurrentItem(item: Family | null) {
      this.currentItem = item;
    },

    async getById(id: string): Promise<Family | undefined> {
      const result = await this.services.family.getById(id);
      if (result.ok) {
        if (result.value) {
          this.currentItem = result.value;
        }
        return result.value;
      } else {
        console.error(`Error fetching item with ID ${id}:`, result.error);
        return undefined;
      }
    },

    async searchLookup(
      filter: FamilyFilter,
      page: number,
      itemsPerPage: number,
    ) {
      this.filter = filter;
      this.currentPage = page;
      this.itemsPerPage = itemsPerPage;
      await this._loadItems();
    },

    async getByIds(ids: string[]): Promise<Family[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.getByIds(ids);
      this.loading = false;
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
});
