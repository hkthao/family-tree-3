import { defineStore } from 'pinia';
import type { FamilyEvent } from '@/types/family-event';
import type { Paginated } from '@/types/pagination';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { EventFilter } from '@/services/family-event/family-event.service.interface';

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
      try {
        const response: Paginated<FamilyEvent> = await this.services.familyEvent.searchItems(
          this.filter,
          this.currentPage,
          this.itemsPerPage
        );
        this.items = response.items;
        this.totalItems = response.totalItems;
        this.totalPages = response.totalPages;
      } catch (e) {
        this.error = 'Không thể tải danh sách sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addItem(newItem: Omit<FamilyEvent, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
        const addedItem = await this.services.familyEvent.add(newItem);
        this.items.push(addedItem);
        await this._loadItems(); // Re-fetch to update pagination and filters
      } catch (e) {
        this.error = 'Không thể thêm sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateItem(updatedItem: FamilyEvent) {
      this.loading = true;
      this.error = null;
      try {
        const updated = await this.services.familyEvent.update(updatedItem);
        const index = this.items.findIndex((item) => item.id === updated.id);
        if (index !== -1) {
          this.items[index] = updated;
          await this._loadItems(); // Re-fetch to update pagination and filters
        } else {
          throw new Error('Không tìm thấy sự kiện gia đình để cập nhật trong kho.');
        }
      } catch (e) {
        this.error = 'Không thể cập nhật sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await this.services.familyEvent.delete(id);
        await this._loadItems(); // Re-fetch to update pagination and filters
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        }
      } catch (e) {
        this.error = 'Không thể xóa sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
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
