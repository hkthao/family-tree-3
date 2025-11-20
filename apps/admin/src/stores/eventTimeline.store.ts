import { defineStore } from 'pinia';
import type { Event, EventFilter } from '@/types';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';

export const useEventTimelineStore = defineStore('eventTimeline', {
  state: () => ({
    events: [] as Event[],
    totalEvents: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    loading: false,
    error: null as string | null,
    filter: {} as EventFilter,
  }),
  getters: {
    paginatedEvents: (state) => state.events,
    paginationLength: (state) => {
      if (typeof state.totalEvents !== 'number' || typeof state.itemsPerPage !== 'number' || state.itemsPerPage <= 0) {
        return 1;
      }
      return Math.max(1, Math.ceil(state.totalEvents / state.itemsPerPage));
    },
  },
  actions: {
    async loadEvents(familyId?: string, memberId?: string) {
      this.loading = true;
      this.error = null;
      const { showSnackbar } = useGlobalSnackbar();
      const { t } = useI18n();

      try {
        const currentFilter: EventFilter = {};

        if (memberId) {
          currentFilter.relatedMemberId = memberId;
        } else if (familyId) {
          currentFilter.familyId = familyId;
        } else {
          this.events = [];
          this.totalEvents = 0;
          this.loading = false;
          return;
        }

        const result = await this.services.event.loadItems(currentFilter, this.currentPage, this.itemsPerPage);

        if (result.ok) {
          this.events = result.value.items;
          this.totalEvents = result.value.totalItems;
        } else {
          this.error = result.error?.message || t('event.timeline.errors.load');
          showSnackbar(this.error || t('common.error'));
          this.events = [];
          this.totalEvents = 0;
        }
      } catch (err: any) {
        this.error = err.message || t('event.timeline.errors.load');
        showSnackbar(this.error || t('common.error'));
        this.events = [];
        this.totalEvents = 0;
      } finally {
        this.loading = false;
      }
    },

    setPage(page: number) {
      this.currentPage = page;
    },

    setItemsPerPage(perPage: number) {
      this.itemsPerPage = perPage;
    },
  },
});
