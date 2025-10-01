import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { EventFilter, Event } from '@/types';
import { defineStore } from 'pinia';

export const useEventStore = defineStore('event', {
  state: () => ({
    items: [] as Event[],
    currentItem: null as unknown as Event,
    loading: false,
    error: null as string | null,
    filter: {} as EventFilter,
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE, // Default items per page
    totalPages: 0,
  }),
  getters: {},
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
        await this._loadItems();
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
        await this._loadItems();
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
        await this._loadItems();
      } else {
        this.error = i18n.global.t('event.errors.delete');
        console.error(result.error);
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

    setCurrentItem(item: Event) {
      this.currentItem = item;
    },

    async getById(id: string): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.getById(id);
      this.loading = false;
      if (result.ok) {
        this.currentItem = { ...(result.value as Event) }; // Set currentItem
      } else {
        this.error = i18n.global.t('event.errors.loadById');
        console.error(result.error);
      }
    },

    async getByIds(ids: string[]): Promise<Event[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.getByIds(ids);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error =
          result.error.message || 'Không thể tải danh sách sự kiện.';
        console.error(result.error);
        return [];
      }
    },
  },
});
