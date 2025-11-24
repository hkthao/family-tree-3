import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { Event, EventFilter } from '@/types';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination'; // Import DEFAULT_ITEMS_PER_PAGE

interface EventTimelineState {
  error: string | null;
  list: {
    items: Event[];
    loading: boolean;
    filters: EventFilter;
    totalItems: number;
    currentPage: number;
    itemsPerPage: number;
    totalPages: number;
    sortBy: { key: string; order: string }[];
  };
}

export const useEventTimelineStore = defineStore('eventTimeline', {
  state: (): EventTimelineState => ({
    error: null,
    list: {
      items: [],
      loading: false,
      filters: {} as EventFilter,
      totalItems: 0,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE, // Use DEFAULT_ITEMS_PER_PAGE
      totalPages: 1,
      sortBy: [],
    },
  }),
  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      // const eventService = useEventService(); // Remove this line

      const result = await this.services.event.loadItems( // Use this.services.event
        this.list.filters,
        this.list.currentPage,
        this.list.itemsPerPage,
        this.list.sortBy,
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('event.errors.load');
        console.error(result.error);
      }
      this.list.loading = false;
    },

    setListOptions(options: {
      page: number;
      itemsPerPage: number;
      sortBy: { key: string; order: string }[];
    }) {
      let changed = false;
      if (this.list.currentPage !== options.page) {
        this.list.currentPage = options.page;
        changed = true;
      }
      if (this.list.itemsPerPage !== options.itemsPerPage) {
        this.list.itemsPerPage = options.itemsPerPage;
        changed = true;
      }
      const currentSortBy = JSON.stringify(this.list.sortBy);
      const newSortBy = JSON.stringify(options.sortBy);
      if (currentSortBy !== newSortBy) {
        this.list.sortBy = options.sortBy;
        changed = true;
      }
      if (changed) {
        this._loadItems();
      }
    },

    setFilters(filters: EventFilter) {
      this.list.filters = { ...this.list.filters, ...filters };
      this.list.currentPage = 1; // Reset to first page when filters change
      this._loadItems();
    },
  },
});