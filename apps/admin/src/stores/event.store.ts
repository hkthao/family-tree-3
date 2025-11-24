import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { EventFilter, Event, Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { defineStore } from 'pinia';

export const useEventStore = defineStore('event', {
  state: () => ({
    error: null as string | null,

    list: {
      items: [] as Event[],
      loading: false,
      filters: {} as EventFilter, // Renamed filter to filters
      totalItems: 0,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[], // Added sortBy
    },

    detail: {
      item: null as unknown as Event,
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

    // eventCache: new IdCache<Event>(), // Remove eventCache from state
  }),
  getters: {},
  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.event.loadItems(
        this.list.filters, // Use filters
        this.list.currentPage,
        this.list.itemsPerPage,
        this.list.sortBy, // Pass sortBy
      );
      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = result.value.totalPages;
        // this.eventCache.setMany(result.value.items); // Remove Cache fetched items
      } else {
        this.error = i18n.global.t('event.errors.load');
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: Omit<Event, 'id'>): Promise<Result<Event, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.event.add(newItem);
      if (result.ok) {
        // this.eventCache.clear(); // Invalidate cache on add
        await this._loadItems();
      } else {
        this.error = i18n.global.t('event.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result; // Return the result
    },

    async addItems(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>> {
      this.add.loading = true;
      this.error = null;
      try {
        const createCommands = newItems.map(item => ({
          name: item.name,
          type: item.type,
          startDate: item.startDate,
          endDate: item.endDate,
          location: item.location,
          description: item.description,
          familyId: item.familyId,
          relatedMembers: item.relatedMembers,
        }));

        // Assuming a bulk add method exists in the service
        const result = await this.services.event.addItems(createCommands);
        if (result.ok) {
          // this.eventCache.clear(); // Invalidate cache on add
          await this._loadItems();
          return result; // Return the result from the service
        } else {
          this.error = i18n.global.t('aiInput.saveError'); // Generic save error for now
          console.error(result.error);
          return result; // Return the error result
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiInput.saveError');
        console.error(err);
        return { ok: false, error: { message: this.error } } as Result<string[], ApiError>; // Return a failure result
      } finally {
        this.add.loading = false;
      }
    },

    async updateItem(updatedItem: Event): Promise<Result<Event, ApiError>> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.event.update(updatedItem);
      if (result.ok) {
        // this.eventCache.clear(); // Invalidate cache on update
        await this._loadItems();
      } else {
        this.error = i18n.global.t('event.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.event.delete(id);
      if (result.ok) {
        // this.eventCache.clear(); // Invalidate cache on delete
        await this._loadItems();
      } else {
        this.error = i18n.global.t('event.errors.delete');
        console.error(result.error);
      }
      this._delete.loading = false;
      return result;
    },

    setPage(page: number) {
      if (page >= 1 && page <= this.list.totalPages && this.list.currentPage !== page) {
        this.list.currentPage = page;
        // _loadItems() will be called by setListOptions
      }
    },

    setItemsPerPage(count: number) {
      if (count > 0 && this.list.itemsPerPage !== count) {
        this.list.itemsPerPage = count;
        this.list.currentPage = 1; // Reset to first page when items per page changes
        // _loadItems() will be called by setListOptions
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.list.sortBy = sortBy;
      this.list.currentPage = 1; // Reset to first page on sort change
      // _loadItems() will be called by setListOptions
    },

    setListOptions(options: {
      page: number;
      itemsPerPage: number;
      sortBy: { key: string; order: string }[];
    }) {
      this.setPage(options.page); // Update page
      this.setItemsPerPage(options.itemsPerPage); // Update items per page
      this.setSortBy(options.sortBy); // Update sort by
      this._loadItems(); // Load items with new options
    },

    setCurrentItem(item: Event) {
      this.detail.item = item;
    },

    async getById(id: string): Promise<Event | undefined> {
      this.detail.loading = true;
      this.error = null;

      const result = await this.services.event.getById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('event.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    async getByIds(ids: string[]): Promise<Event[]> {
      this.list.loading = true;
      this.error = null;

      const result = await this.services.event.getByIds(ids);

      this.list.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách sự kiện.';
        console.error(result.error);
        return [];
      }
    },

    async fetchUpcomingEvents(familyId?: string): Promise<Event[]> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.event.getUpcomingEvents(familyId);
      this.list.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = result.error.message || 'Failed to fetch upcoming events.';
        console.error(result.error);
        return [];
      }
    },
  },
});
