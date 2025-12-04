// apps/admin/src/stores/prompt.store.ts

import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Prompt, PromptFilter } from '@/types/prompt';
import type { Result } from '@/types';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';


export const usePromptStore = defineStore('prompt', {
  state: () => ({
    // General state
    error: null as string | null,

    // State for list operations
    list: {
      items: [] as Prompt[],
      loading: false,
      filters: {
        searchQuery: '',
      } as PromptFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[],
    },

    // State for single item operations
    detail: {
      item: null as Prompt | null,
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
      const filters: PromptFilter = {
        searchQuery: this.list.filters.searchQuery,
        sortBy: this.list.sortBy.length > 0 ? this.list.sortBy[0].key : undefined,
        sortOrder: this.list.sortBy.length > 0 ? (this.list.sortBy[0].order as 'asc' | 'desc') : undefined,
      };

      const result = await this.services.prompt.getPaginated(
        filters,
        this.list.currentPage,
        this.list.itemsPerPage,
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = Math.ceil(this.list.totalItems / this.list.itemsPerPage);
        this.list.currentPage = result.value.page;
      } else {
        this.error = i18n.global.t('prompt.errors.load');
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        this.list.currentPage = 1;
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: Omit<Prompt, 'id'>): Promise<Result<string, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.prompt.add(newItem);
      if (result.ok) {
        // No need to reload all items, as the new item is usually added to the list in the UI directly or the list is re-fetched after closing the form.
      } else {
        this.error = i18n.global.t('prompt.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: Prompt): Promise<Result<void, ApiError>> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.prompt.update(updatedItem);
      if (!result.ok) {
        this.error = i18n.global.t('prompt.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.prompt.delete(id);
      if (!result.ok) {
        this.error = result.error.message || i18n.global.t('prompt.errors.delete');
        console.error(result.error);
      }
      this._delete.loading = false;
      return result;
    },

    setListOptions(options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) {
      if (this.list.currentPage !== options.page) {
        this.list.currentPage = options.page;
      }

      if (this.list.itemsPerPage !== options.itemsPerPage) {
        this.list.itemsPerPage = options.itemsPerPage;
      }

      const newSortBy = Array.isArray(options.sortBy) ? options.sortBy : [];
      const currentSortBy = JSON.stringify(this.list.sortBy);
      if (currentSortBy !== JSON.stringify(newSortBy)) {
        this.list.sortBy = newSortBy;
      }

      this._loadItems();
    },

    async setCurrentItem(item: Prompt | null) {
      this.detail.item = item;
    },

    async getById(id: string, code?: string): Promise<Prompt | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.prompt.getById(id, code);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('prompt.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    clearItems() {
      this.list.items = [];
      this.list.totalItems = 0;
      this.list.totalPages = 1;
      this.list.currentPage = 1;
    }
  },
});
