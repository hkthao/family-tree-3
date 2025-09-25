import { defineStore } from 'pinia';
import type { Family, FamilySearchFilter } from '@/types/family';
import type { Paginated } from '@/types/common';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { ApiError } from '@/utils/api';

export const useFamilyStore = defineStore('family', {
  state: () => ({
    items: [] as Family[],
    currentItem: null as Family | null,
    loading: false,
    error: null as string | null,
    filter: {} as FamilySearchFilter, // New filter object
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE, // Default items per page
    totalPages: 1,
    itemCache: {} as Record<string, Family>, // Cache for individual items
  }),
  getters: {
    getItemById: (state) => (id: string) => {
      return state.items.find((item) => item.id === id);
    },
    paginatedItems: (state) => {
      const start = (state.currentPage - 1) * state.itemsPerPage;
      const end = start + state.itemsPerPage;
      return state.items.slice(start, end);
    },
  },
  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.searchItems(
        this.filter,
        this.currentPage,
        this.itemsPerPage,
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách gia đình.';
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
        this.items.push(result.value);
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = result.error.message || 'Không thể thêm gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: Family) {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.update(updatedItem);
      if (result.ok) {
        const index = this.items.findIndex((item) => item.id === result.value.id);
        if (index !== -1) {
          this.items[index] = result.value;
          await this._loadItems(); // Re-fetch to update pagination and filters
        } else {
          this.error = 'Không tìm thấy gia đình để cập nhật trong kho.';
        }
      } else {
        this.error = result.error.message || 'Không thể cập nhật gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.delete(id);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        }
      } else {
        this.error = result.error.message || 'Không thể xóa gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async searchItems(
      filter: FamilySearchFilter,
    ) {
      this.filter = filter;
      this.currentPage = 1; // Reset to first page on new search
      await this._loadItems(); // Trigger fetch with new search terms
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        await this._loadItems(); // Trigger fetch with new page
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0) {
        this.itemsPerPage = count;
        this.currentPage = 1; // Reset to first page when items per page changes
        await this._loadItems(); // Trigger fetch with new items per page
      }
    },

    setCurrentItem(item: Family | null) {
      this.currentItem = item;
    },

    async fetchItemById(id: string): Promise<Family | undefined> {
      if (this.itemCache[id]) {
        return this.itemCache[id];
      }
      const result = await this.services.family.getById(id);
      if (result.ok) {
        if (result.value) {
          this.itemCache[id] = result.value;
        }
        return result.value;
      } else {
        console.error(`Error fetching item with ID ${id}:`, result.error);
        return undefined;
      }
    },

    async fetchAllItems() {
      this.loading = true;
      this.error = null;
      // Use a large itemsPerPage to fetch all items
      const result = await this.services.family.searchItems(
        { searchQuery: '', visibility: 'all' },
        1,
        1000,
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = 1;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async searchLookup(filter: FamilySearchFilter, page: number, itemsPerPage: number) {
      this.filter = filter;
      this.currentPage = page;
      this.itemsPerPage = itemsPerPage;
      await this._loadItems();
    },

    async getManyByIds(ids: string[]): Promise<Family[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.family.getManyByIds(ids);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách gia đình.';
        console.error(result.error);
        return [];
      }
    },
  },
});
