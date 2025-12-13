import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyLinkDto, FamilyLinkFilter, Result } from '@/types';

export const useFamilyLinkStore = defineStore('familyLink', {
  state: () => ({
    error: null as string | null,
    list: {
      items: [] as FamilyLinkDto[],
      loading: false,
      filters: {
        familyId: undefined,
        searchQuery: '',
        otherFamilyId: null,
      } as FamilyLinkFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[],
    },
    detail: {
      item: null as FamilyLinkDto | null,
      loading: false,
    },
    add: {
      loading: false,
    },
  }),

  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      try {
        const linksResult = await this.services.familyLink.getFamilyLinks(
          this.list.filters.familyId as string,
          {
            searchQuery: this.list.filters.searchQuery,
            otherFamilyId: this.list.filters.otherFamilyId,
            sortBy: this.list.sortBy.length > 0 ? this.list.sortBy[0].key : undefined,
            sortOrder: this.list.sortBy.length > 0 ? (this.list.sortBy[0].order as 'asc' | 'desc') : undefined,
          },
          this.list.currentPage,
          this.list.itemsPerPage,
        );

        if (!linksResult.ok) {
            this.error = linksResult.error?.message || i18n.global.t('familyLink.errors.load');
            console.error(linksResult.error);
            return; // Exit if links failed
        }

        this.list.items = linksResult.value?.items || [];
        this.list.totalItems = linksResult.value?.totalItems || 0;
        this.list.totalPages = linksResult.value?.totalPages || 1;
      } catch (err: any) {
        this.error = err.message || i18n.global.t('familyLink.errors.load');
        console.error(err);
      } finally {
        this.list.loading = false;
      }
    },

    setListOptions(options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) {
      if (this.list.currentPage !== options.page) {
        this.list.currentPage = options.page;
      }
      if (this.list.itemsPerPage !== options.itemsPerPage) {
        this.list.itemsPerPage = options.itemsPerPage;
      }
      const currentSortBy = JSON.stringify(this.list.sortBy);
      const newSortBy = JSON.stringify(options.sortBy);
      if (currentSortBy !== newSortBy) {
        this.list.sortBy = options.sortBy;
      }
      this._loadItems();
    },

    setFilters(filters: FamilyLinkFilter) {
      this.list.filters = { ...this.list.filters, ...filters };
      this._loadItems();
    },

    async deleteFamilyLink(familyLinkId: string): Promise<Result<void>> {
      this.add.loading = true; // Use add loading for actions
      this.error = null;
      const result = await this.services.familyLink.deleteFamilyLink(familyLinkId);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error?.message || i18n.global.t('familyLink.links.messages.deleteError');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async getById(id: string): Promise<FamilyLinkDto | undefined> {
        this.detail.loading = true;
        this.error = null;
        let item: FamilyLinkDto | null = null;
        
        const linkResult = await this.services.familyLink.getFamilyLinkById(id);
        if (!linkResult.ok) {
            this.error = linkResult.error?.message || i18n.global.t('familyLink.errors.loadById');
            console.error(linkResult.error);
        } else if (linkResult.value) {
            item = linkResult.value;
        }
        
        this.detail.item = item;
        this.detail.loading = false;
        return item ?? undefined;
    },
  },
});
