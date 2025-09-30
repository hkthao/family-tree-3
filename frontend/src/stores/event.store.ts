import { DEFAULT_ITEMS_PER_PAGE } from "@/constants/pagination";
import i18n from "@/plugins/i18n";
import type { EventFilter, Event } from "@/types";
import { defineStore } from "pinia";

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
  },
  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.loadItems(
        this.filter,
        this.currentPage,
        this.itemsPerPage,
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('event.errors.load');
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
        this.error = i18n.global.t('event.errors.add');
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: Event) {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.update(updatedItem);
      if (result.ok) {
        const index = this.items.findIndex(
          (item) => item.id === result.value.id,
        );
        if (index !== -1) {
          this.items[index] = result.value;
          await this._loadItems(); // Re-fetch to update pagination and filters
        } else {
          this.error = i18n.global.t('event.errors.notFound');
        }
      } else {
        this.error = i18n.global.t('event.errors.update');
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
        this.error = i18n.global.t('event.errors.delete');
        console.error(result.error);
      }
      this.loading = false;
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

    setCurrentItem(item: Event | null) {
      this.currentItem = item;
    },

    async getItemById(id: string): Promise<Event | undefined> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.getById(id);
      this.loading = false;
      if (result.ok) {
        this.currentItem = result.value as Event; // Set currentItem
        return result.value;
      } else {
        this.error = i18n.global.t('event.errors.loadById');
        console.error(result.error);
        return undefined;
      }
    },
  },
});
