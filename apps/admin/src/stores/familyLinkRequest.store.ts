import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { FamilyLinkRequestDto, FamilyLinkRequestFilter, Result } from '@/types';

export const useFamilyLinkRequestStore = defineStore('familyLinkRequest', {
  state: () => ({
    error: null as string | null,
    list: {
      items: [] as FamilyLinkRequestDto[],
      loading: false,
      filters: {
        familyId: undefined,
        searchQuery: '',
        status: undefined,
        otherFamilyId: null,
      } as FamilyLinkRequestFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[],
    },
    detail: {
      item: null as FamilyLinkRequestDto | null,
      loading: false,
    },
    add: {
      loading: false,
    },
    _delete: {
      loading: false,
    },
  }),

  actions: {
    async _loadItems(familyId: string) {
      this.list.loading = true;
      this.error = null;
      try {
        const result = await this.services.familyLinkRequest.searchFamilyLinkRequests(
          familyId,
          {
            searchQuery: this.list.filters.searchQuery,
            status: this.list.filters.status,
            otherFamilyId: this.list.filters.otherFamilyId,
            sortBy: this.list.sortBy.length > 0 ? this.list.sortBy[0].key : undefined,
            sortOrder: this.list.sortBy.length > 0 ? (this.list.sortBy[0].order as 'asc' | 'desc') : undefined,
          },
          this.list.currentPage,
          this.list.itemsPerPage,
        );

        if (!result.ok) {
            this.error = result.error?.message || i18n.global.t('familyLink.errors.load');
            console.error(result.error);
            this.list.items = [];
            this.list.totalItems = 0;
            this.list.totalPages = 1;
            return;
        }

        this.list.items = result.value?.items || [];
        this.list.totalItems = result.value?.totalItems || 0;
        this.list.totalPages = result.value?.totalPages || 1;
      } catch (err: any) {
        this.error = err.message || i18n.global.t('familyLink.errors.load');
        console.error(err);
      } finally {
        this.list.loading = false;
      }
    },

    setListOptions(familyId: string, options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) {
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
      this._loadItems(familyId);
    },

    setFilters(familyId: string, filters: FamilyLinkRequestFilter) {
      this.list.filters = { ...this.list.filters, ...filters };
      this._loadItems(familyId);
    },

    async getById(id: string): Promise<FamilyLinkRequestDto | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.familyLinkRequest.getFamilyLinkRequestById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('familyLink.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    async createRequest(requestingFamilyId: string, targetFamilyId: string, requestMessage?: string): Promise<Result<string>> {
        this.add.loading = true;
        this.error = null;
        const result = await this.services.familyLinkRequest.createFamilyLinkRequest(requestingFamilyId, targetFamilyId, requestMessage);
        if (result.ok) {
            await this._loadItems(requestingFamilyId); // Reload requesting family's requests
        } else {
            this.error = result.error?.message || i18n.global.t('familyLink.requests.messages.sendError');
            console.error(result.error);
        }
        this.add.loading = false;
        return result;
    },

    async deleteRequest(id: string, familyId: string): Promise<Result<void>> {
        this._delete.loading = true;
        this.error = null;
        const result = await this.services.familyLinkRequest.deleteFamilyLinkRequest(id);
        if (result.ok) {
            await this._loadItems(familyId);
        } else {
            this.error = result.error?.message || i18n.global.t('familyLink.requests.messages.deleteError');
            console.error(result.error);
        }
        this._delete.loading = false;
        return result;
    },

    async approveRequest(requestId: string, familyId: string, responseMessage?: string): Promise<Result<void>> {
        // Use add.loading as a general action loading indicator
        this.add.loading = true;
        this.error = null;
        const result = await this.services.familyLinkRequest.approveFamilyLinkRequest(requestId, responseMessage);
        if (result.ok) {
            await this._loadItems(familyId);
        } else {
            this.error = result.error?.message || i18n.global.t('familyLink.requests.messages.approveError');
            console.error(result.error);
        }
        this.add.loading = false;
        return result;
    },

    async rejectRequest(requestId: string, familyId: string, responseMessage?: string): Promise<Result<void>> {
        // Use add.loading as a general action loading indicator
        this.add.loading = true;
        this.error = null;
        const result = await this.services.familyLinkRequest.rejectFamilyLinkRequest(requestId, responseMessage);
        if (result.ok) {
            await this._loadItems(familyId);
        } else {
            this.error = result.error?.message || i18n.global.t('familyLink.requests.messages.rejectError');
            console.error(result.error);
        }
        this.add.loading = false;
        return result;
    },
  },
});
