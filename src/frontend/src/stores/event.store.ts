import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { EventFilter, Event, Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { defineStore } from 'pinia';
import { IdCache } from '@/utils/cacheUtils'; // Import IdCache

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
    totalPages: 1,
    eventCache: new IdCache<Event>(), // Add eventCache to state
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
        this.eventCache.setMany(result.value.items); // Cache fetched items
      } else {
        this.error = i18n.global.t('event.errors.load');
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<Event, 'id'>): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.add(newItem);
      if (result.ok) {
        this.eventCache.clear(); // Invalidate cache on add
        await this._loadItems();
      } else {
        this.error = i18n.global.t('event.errors.add');
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItems(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>> {
      this.loading = true;
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
          this.eventCache.clear(); // Invalidate cache on add
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
        this.loading = false;
      }
    },

    async updateItem(updatedItem: Event): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.update(updatedItem);
      if (result.ok) {
        this.eventCache.clear(); // Invalidate cache on update
        await this._loadItems();
      } else {
        this.error = i18n.global.t('event.errors.update');
        console.error(result.error);
      }
      this.loading = false;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.delete(id);
      if (result.ok) {
        this.eventCache.clear(); // Invalidate cache on delete
        await this._loadItems();
      } else {
        this.error = i18n.global.t('event.errors.delete');
        console.error(result.error);
      }
      this.loading = false;
      return result;
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
      // Assuming EventFilter has sortBy and sortOrder properties
      this.filter.sortBy = sortBy.length > 0 ? sortBy[0].key : undefined;
      this.filter.sortOrder =
        sortBy.length > 0 ? (sortBy[0].order as 'asc' | 'desc') : undefined;
      this.currentPage = 1; // Reset to first page on sort change
      this._loadItems();
    },

    setCurrentItem(item: Event) {
      this.currentItem = item;
    },

    async getById(id: string): Promise<Event | undefined> {
      this.loading = true;
      this.error = null;

      const cachedEvent = this.eventCache.get(id);
      if (cachedEvent) {
        this.loading = false;
        this.currentItem = cachedEvent;
        return cachedEvent;
      }

      const result = await this.services.event.getById(id);
      this.loading = false;
      if (result.ok) {
        if (result.value) {
          this.currentItem = result.value;
          this.eventCache.set(result.value); // Cache the fetched event
          return result.value;
        }
      } else {
        this.error = i18n.global.t('event.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    async getByIds(ids: string[]): Promise<Event[]> {
      this.loading = true;
      this.error = null;

      const result = await this.eventCache.getMany(ids, (missingIds) =>
        this.services.event.getByIds(missingIds),
      );

      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách sự kiện.';
        console.error(result.error);
        return [];
      }
    },

    async fetchUpcomingEvents(familyId?: string): Promise<Event[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.event.getUpcomingEvents(familyId);
      this.loading = false;
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
