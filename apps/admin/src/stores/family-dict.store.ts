import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { FamilyDict, FamilyDictFilter, FamilyDictImport, Result } from '@/types';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';


export const useFamilyDictStore = defineStore('familyDict', {
  state: () => ({
    // General state
    error: null as string | null,

    // State for list operations
    list: {
      items: [] as FamilyDict[],
      loading: false,
      filters: {
        searchQuery: '',
      } as FamilyDictFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[],
    },

    // State for single item operations
    detail: {
      item: null as FamilyDict | null,
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
      const result = await this.services.familyDict.loadItems({
        ...this.list.filters,
        sortBy: this.list.sortBy.length > 0 ? this.list.sortBy[0].key : undefined,
        sortOrder: this.list.sortBy.length > 0 ? (this.list.sortBy[0].order as 'asc' | 'desc') : undefined,
      },
        this.list.currentPage,
        this.list.itemsPerPage,
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = Math.ceil(this.list.totalItems / this.list.itemsPerPage);
        this.list.currentPage = result.value.page;
      } else {
        this.error = i18n.global.t('familyDict.errors.load');
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        this.list.currentPage = 1;
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: Omit<FamilyDict, 'id'>): Promise<Result<FamilyDict, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.familyDict.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('familyDict.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: FamilyDict): Promise<Result<FamilyDict, ApiError>> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.familyDict.update(updatedItem);
      if (result.ok) {
       await this._loadItems();
      } else {
        this.error = i18n.global.t('familyDict.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.familyDict.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || 'Failed to delete familyDict.';
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

    async setCurrentItem(item: FamilyDict | null) {
      this.detail.item = item;
    },

    async getById(id: string): Promise<FamilyDict | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.familyDict.getById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('familyDict.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    async getByIds(ids: string[]): Promise<Result<FamilyDict[], ApiError>> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.familyDict.getByIds(ids);
      this.list.loading = false;
      if (result.ok) {
        this.list.items = result.value;
      } else {
        this.error = i18n.global.t('familyDict.errors.load');
        console.error(result.error);
      }
      return result;
    },

    async importItems(data: FamilyDictImport): Promise<Result<string[], ApiError>> {
      this.add.loading = true; // Use add.loading for import as well
      this.error = null;
      const result = await this.services.familyDict.importItems(data);
      if (result.ok) {
        await this._loadItems(); // Reload list after successful import
      } else {
        this.error = result.error?.message || i18n.global.t('familyDict.errors.import');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    clearItems() {
      this.list.items = [];
      this.list.totalItems = 0;
      this.list.totalPages = 1;
      this.list.currentPage = 1;
    }
  },
});