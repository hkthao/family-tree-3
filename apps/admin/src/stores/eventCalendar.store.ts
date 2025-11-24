import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { Event, EventFilter } from '@/types';
import { startOfMonth, endOfMonth } from 'date-fns'; // Import date-fns utilities

interface EventCalendarState {
  error: string | null;
  list: {
    items: Event[];
    loading: boolean;
    filters: EventFilter;
    currentMonthStartDate: Date | null;
    currentMonthEndDate: Date | null;
  };
}

export const useEventCalendarStore = defineStore('eventCalendar', {
  state: (): EventCalendarState => ({
    error: null,
    list: {
      items: [],
      loading: false,
      filters: {} as EventFilter,
      currentMonthStartDate: null,
      currentMonthEndDate: null,
    },
  }),
  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;

      // Ensure date range is set for the current month
      if (!this.list.currentMonthStartDate || !this.list.currentMonthEndDate) {
        this.list.loading = false;
        return;
      }

      const filters: EventFilter = {
        ...this.list.filters,
        startDate: this.list.currentMonthStartDate,
        endDate: this.list.currentMonthEndDate,
      };

      // Always fetch all events for the current month in calendar view
      const page = 1;
      const itemsPerPage = 100; // Default to 100 instead of fetching all

      const result = await this.services.event.loadItems(
        filters,
        page,
        itemsPerPage, // This is where 100 is used
      );

      if (result.ok) {
        this.list.items = result.value.items;
      } else {
        this.error = i18n.global.t('event.errors.load');
        console.error(result.error);
      }
      this.list.loading = false;
    },

    setFilters(filters: EventFilter) {
      this.list.filters = { ...this.list.filters, ...filters };
      this._loadItems();
    },

    setCurrentMonth(date: Date) {
      this.list.currentMonthStartDate = startOfMonth(date);
      this.list.currentMonthEndDate = endOfMonth(date);
      this._loadItems();
    },
  },
});
