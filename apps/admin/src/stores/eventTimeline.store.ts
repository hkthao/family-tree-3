import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { Event, EventFilter, Member, RelatedMember } from '@/types';
import { Gender } from '@/types'; // Import Member and RelatedMember
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

      const result = await this.services.event.search(
        {
          page: this.list.currentPage,
          itemsPerPage: this.list.itemsPerPage,
          sortBy: this.list.sortBy,
        },
        this.list.filters
      );

      if (result.ok) {
        let fetchedEvents = result.value.items;

        // Collect all unique relatedMemberIds
        const allRelatedMemberIds = new Set<string>();
        fetchedEvents.forEach(event => {
          event.relatedMemberIds?.forEach(memberId => allRelatedMemberIds.add(memberId));
        });

        if (allRelatedMemberIds.size > 0) {
          // Fetch full member objects for these IDs
          const membersResult = await this.services.member.getByIds(Array.from(allRelatedMemberIds));
          if (membersResult.ok) {
            const memberMap = new Map<string, Member>();
            membersResult.value.forEach(member => memberMap.set(member.id, member));

            // Hydrate relatedMembers for each event
            fetchedEvents = fetchedEvents.map(event => {
              const relatedMembers: RelatedMember[] = [];
              event.relatedMemberIds?.forEach(memberId => {
                const member = memberMap.get(memberId);
                if (member) {
                  relatedMembers.push({
                    id: member.id,
                    fullName: member.fullName ?? '', // Provide fallback for fullName
                    avatarUrl: member.avatarUrl,
                    gender: member.gender ?? Gender.Other, // Provide fallback for gender
                  });
                }
              });
              return { ...event, relatedMembers };
            });
          } else {
            console.error('Failed to fetch related members for events:', membersResult.error);
          }
        }

        this.list.items = fetchedEvents;
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