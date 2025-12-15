import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { Event, EventFilter, Member, RelatedMember } from '@/types';
import { Gender } from '@/types'; // Import Member and RelatedMember
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination'; // Import DEFAULT_ITEMS_PER_PAGE
import { CalendarType } from '@/types/enums'; // Import CalendarType

interface EventCalendarState {
  error: string | null;
  list: {
    items: Event[];
    loading: boolean;
    filters: EventFilter;
    totalItems: number; // Added to interface
    currentPage: number; // Added to interface
    itemsPerPage: number; // Explicitly define itemsPerPage
    totalPages: number; // Added to interface
    sortBy: any[]; // Added to interface (can be more specific if sort options are known)
    minSolarDate: Date | null; // New filter
    maxSolarDate: Date | null; // New filter
    calendarType: CalendarType | null; // New filter
  };
}

export const useEventCalendarStore = defineStore('eventCalendar', {
  state: (): EventCalendarState => ({
    error: null,
    list: {
      items: [],
      loading: false,
      filters: {} as EventFilter,
      totalItems: 0, // Added
      currentPage: 1, // Added
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE, // Initialize itemsPerPage
      totalPages: 1, // Added
      sortBy: [], // Added
      minSolarDate: null,
      maxSolarDate: null,
      calendarType: null,
    },
  }),
  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;

      const filters: EventFilter = {
        ...this.list.filters,
        minSolarDate: this.list.minSolarDate,
        maxSolarDate: this.list.maxSolarDate,
        calendarType: this.list.calendarType,
      };

      // Always fetch all events for the current month in calendar view
      const page = 1;
      const itemsPerPage = 100; // Default to 100 instead of fetching all

      const result = await this.services.event.search(
        {
          page: page,
          itemsPerPage: itemsPerPage,
        },
        filters
      );

      if (result.ok) {
        this.list.items = result.value.items;
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
      if (filters.minSolarDate !== undefined) this.list.minSolarDate = filters.minSolarDate;
      if (filters.maxSolarDate !== undefined) this.list.maxSolarDate = filters.maxSolarDate;
      if (filters.calendarType !== undefined) this.list.calendarType = filters.calendarType;
      this.list.currentPage = 1; // Reset to first page when filters change
      this._loadItems();
    },

    // Remove setCurrentMonth action as date ranges are handled by minSolarDate and maxSolarDate
  },
});
