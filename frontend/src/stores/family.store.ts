import { defineStore } from 'pinia';
import type { Family, FamilySearchFilter } from '@/types/family';
import type { Paginated } from '@/types/pagination';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

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
      try {
        const response: Paginated<Family> =
          await this.services.family.searchItems(
            this.filter,
            this.currentPage,
            this.itemsPerPage,
          );
        this.items = response.items;
        this.totalItems = response.totalItems;
        this.totalPages = response.totalPages;
      } catch (e) {
        this.error = 'Không thể tải danh sách gia đình.';
        this.items = []; // Clear items on error
        this.totalItems = 0; // Reset totalItems on error
        this.totalPages = 1; // Reset totalPages on error
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addItem(newItem: Omit<Family, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
        const addedItem = await this.services.family.add(newItem);
        this.items.push(addedItem);
        await this._loadItems(); // Re-fetch to update pagination and filters
      } catch (e) {
        this.error = 'Không thể thêm gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateItem(updatedItem: Family) {
      this.loading = true;
      this.error = null;
      try {
        const updated = await this.services.family.update(updatedItem);
        const index = this.items.findIndex((item) => item.id === updated.id);
        if (index !== -1) {
          this.items[index] = updated;
          await this._loadItems(); // Re-fetch to update pagination and filters
        } else {
          throw new Error('Không tìm thấy gia đình để cập nhật trong kho.');
        }
      } catch (e) {
        this.error = 'Không thể cập nhật gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await this.services.family.delete(id);
        await this._loadItems(); // Re-fetch to update pagination and filters
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        }
      } catch (e) {
        this.error = 'Không thể xóa gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
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
      try {
        const item = await this.services.family.getById(id);
        if (item) {
          this.itemCache[id] = item;
        }
        return item;
      } catch (e) {
        console.error(`Error fetching item with ID ${id}:`, e);
        return undefined;
      }
    },

    async fetchAllItems() {
      this.loading = true;
      this.error = null;
      try {
        // Use a large itemsPerPage to fetch all items
        const response: Paginated<Family> =
          await this.services.family.searchItems(
            '',
            'all',
            1,
            1000, // A large number
          );
        this.items = response.items;
        this.totalItems = response.totalItems;
        this.totalPages = 1;
      } catch (e) {
        this.error = 'Không thể tải danh sách gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async searchLookup(filter: FamilySearchFilter, page: number, itemsPerPage: number) {
      this.filter = filter;
      this.currentPage = page;
      this.itemsPerPage = itemsPerPage;
      await this._loadItems();
    },
  },
});
