import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Relationship, RelationshipFilter, Result } from '@/types'; // Assuming Relationship and RelationshipFilter types exist
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';

export const useRelationshipStore = defineStore('relationship', {
  state: () => ({
    // General state
    error: null as string | null,

    // State for list operations
    list: {
      items: [] as Relationship[],
      loading: false,
      filters: {
        searchQuery: '',
        familyId: undefined,
        sourceMemberId: undefined,
        targetMemberId: undefined,
        type: undefined,
      } as RelationshipFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[],
    },

    // State for single item operations
    detail: {
      item: null as Relationship | null,
      loading: false,
    },

    // State for add operations
    add: {
      loading: false,
    },

    // State for update operations
    update: {
      loading: false,
    },

    // State for delete operations
    _delete: {
      loading: false,
    },
  }),

  getters: {},

  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.relationship.search(
        {
          page: this.list.currentPage,
          itemsPerPage: this.list.itemsPerPage,
          sortBy: this.list.sortBy.map(s => ({ key: s.key, order: s.order as 'asc' | 'desc' })),
        },
        this.list.filters
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('relationship.errors.load');
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: Omit<Relationship, 'id'>): Promise<Result<Relationship, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.relationship.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('relationship.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async addItems(newItems: Omit<Relationship, 'id'>[]): Promise<Result<string[], ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.relationship.addItems(newItems);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('relationship.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: Relationship): Promise<Result<Relationship, ApiError>> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.relationship.update(updatedItem);
      if (result.ok) {
       await this._loadItems();
      } else {
        this.error = i18n.global.t('relationship.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.relationship.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || 'Failed to delete relationship.';
        console.error(result.error);
      }
      this._delete.loading = false;
      return result;
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.list.totalPages && this.list.currentPage !== page) {
        this.list.currentPage = page;
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0 && this.list.itemsPerPage !== count) {
        this.list.itemsPerPage = count;
        this.list.currentPage = 1;
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.list.sortBy = sortBy;
      this.list.currentPage = 1;
    },

    setListOptions(options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) {
      if (this.list.currentPage !== options.page) {
        this.list.currentPage = options.page;
      }

      const currentSortBy = JSON.stringify(this.list.sortBy);
      const newSortBy = JSON.stringify(options.sortBy);
      if (currentSortBy !== newSortBy) {
        this.list.sortBy = options.sortBy;
      }

      this._loadItems();
    },

    async setCurrentItem(item: Relationship | null) {
      this.detail.item = item;
    },

    async getById(id: string): Promise<Relationship | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.relationship.getById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('relationship.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    clearItems() {
      this.list.items = [];
      this.list.totalItems = 0;
      this.list.totalPages = 1;
    },
  },
});
