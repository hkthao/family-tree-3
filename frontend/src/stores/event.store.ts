import { defineStore } from 'pinia';
import type { Event } from '@/types/event/event';
import type { Paginated } from '@/types/common';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { EventFilter } from '@/services/event/event.service.interface';
import type { ApiError } from '@/utils/api';

export const useEventStore = defineStore('event', {
  state: () => ({
    items: [] as Event[],
    currentItem: null as Event | null,
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
      const result = await this.services.event.searchItems(
        this.filter,
        this.currentPage,
        this.itemsPerPage
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách sự kiện.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<Event, 'id'>) {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.add(newItem);
      if (result.ok) {
        this.items.push(result.value);
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = result.error.message || 'Không thể thêm sự kiện.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: Event) {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.update(updatedItem);
      if (result.ok) {
        const index = this.items.findIndex((item) => item.id === result.value.id);
        if (index !== -1) {
          this.items[index] = result.value;
          await this._loadItems(); // Re-fetch to update pagination and filters
        } else {
          this.error = 'Không tìm thấy sự kiện để cập nhật trong kho.';
        }
      } else {
        this.error = result.error.message || 'Không thể cập nhật sự kiện.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.delete(id);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        }
      } else {
        this.error = result.error.message || 'Không thể xóa sự kiện.';
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

    async setCurrentItem(item: Event | null) {
      this.currentItem = item;
    },

    async fetchItemById(id: string): Promise<Event | undefined> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.getById(id);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = result.error.message || 'Không thể tải sự kiện.';
        console.error(result.error);
        return undefined;
      }
    },
  },
});
