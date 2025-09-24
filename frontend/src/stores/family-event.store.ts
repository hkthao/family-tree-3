import { defineStore } from 'pinia';
import type { FamilyEvent } from '@/types/family-event';
import type { Paginated } from '@/types/pagination';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { EventFilter } from '@/services/family-event/family-event.service.interface';
import type { ApiError } from '@/utils/api';

export const useFamilyEventStore = defineStore('familyEvent', {
  state: () => ({
    items: [] as FamilyEvent[],
    currentItem: null as FamilyEvent | null,
    loading: false,
    error: null as string | null,
    filter: {} as EventFilter,
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE, // Default items per page
    totalPages: 0,
  }),
  getters: {
    getItemById: (state) => (id: string) => {
      return state.items.find((item) => item.id === id);
    },
    paginatedItems: (state) => {
      return state.items; // Events are already paginated by the service
    },
  },
  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.familyEvent.searchItems(
        this.filter,
        this.currentPage,
        this.itemsPerPage
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách sự kiện gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<FamilyEvent, 'id'>) {
      this.loading = true;
      this.error = null;
      const result = await this.services.familyEvent.add(newItem);
      if (result.ok) {
        this.items.push(result.value);
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = result.error.message || 'Không thể thêm sự kiện gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: FamilyEvent) {
      this.loading = true;
      this.error = null;
      const result = await this.services.familyEvent.update(updatedItem);
      if (result.ok) {
        const index = this.items.findIndex((item) => item.id === result.value.id);
        if (index !== -1) {
          this.items[index] = result.value;
          await this._loadItems(); // Re-fetch to update pagination and filters
        } else {
          this.error = 'Không tìm thấy sự kiện gia đình để cập nhật trong kho.';
        }
      } else {
        this.error = result.error.message || 'Không thể cập nhật sự kiện gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      const result = await this.services.familyEvent.delete(id);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        }
      } else {
        this.error = result.error.message || 'Không thể xóa sự kiện gia đình.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async searchItems(filters: EventFilter) {
      this.filter = filters;
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

    setCurrentItem(item: FamilyEvent | null) {
      this.currentItem = item;
    },
  },
});
