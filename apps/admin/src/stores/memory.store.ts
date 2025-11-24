import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Result, Paginated } from '@/types';
import type { MemoryDto, CreateMemoryDto, UpdateMemoryDto } from '@/types/memory';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';
import type { MemoryFilter } from '@/services/memory/memory.service.interface';

export const useMemoryStore = defineStore('memory', {
  state: () => ({
    // General state
    error: null as string | null,

    // State for list operations
    list: {
      items: [] as MemoryDto[],
      loading: false, // Loading state for the list of memories
      filters: {
        memberId: undefined, // Default to undefined for filtering
        searchQuery: '',
      } as MemoryFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[], // Sorting key and order
    },

    // State for single item operations
    detail: {
      item: null as MemoryDto | null,
      loading: false, // Loading state for a single memory
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

  getters: {
    headers: () => {
      const t = i18n.global.t;
      return [
        {
          title: t('memory.list.header.title'),
          align: 'start' as const,
        },
        {
          title: t('member.list.headers.fullName'), // Assuming we want to show member name
          key: 'memberName',
          align: 'start' as const,
        },
        {
          title: t('memory.list.header.tags'),
          key: 'tags',
          sortable: false,
          align: 'start' as const,
        },
        {
          title: t('memory.list.header.createdAt'),
          key: 'createdAt',
          align: 'end' as const,
        },
        {
          title: t('memory.list.header.actions'),
          key: 'actions',
          sortable: false,
          align: 'end' as const,
        },
      ];
    },
  },

  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      
      if (!this.list.filters.memberId) {
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        this.list.loading = false;
        return;
      }

      const result = await this.services.memory.loadItems(
        {
          memberId: this.list.filters.memberId,
          searchQuery: this.list.filters.searchQuery,
          sortBy: this.list.sortBy.length > 0 ? this.list.sortBy[0].key : undefined,
          sortOrder: this.list.sortBy.length > 0 ? (this.list.sortBy[0].order as 'asc' | 'desc') : undefined,
        },
        this.list.currentPage,
        this.list.itemsPerPage,
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('memory.errors.load');
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: CreateMemoryDto): Promise<Result<MemoryDto, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.memory.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('memory.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: UpdateMemoryDto): Promise<Result<MemoryDto, ApiError>> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.memory.update(updatedItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('memory.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.memory.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || i18n.global.t('memory.errors.delete');
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

      const currentSortBy = JSON.stringify(this.list.sortBy);
      const newSortBy = JSON.stringify(options.sortBy);
      if (currentSortBy !== newSortBy) {
        this.list.sortBy = options.sortBy;
      }

      this._loadItems();
    },

    async getById(id: string): Promise<MemoryDto | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.memory.getById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('memory.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    setFilters(filters: MemoryFilter) {
      this.list.filters = { ...this.list.filters, ...filters };
    },

    async analyzePhoto(command: FormData): Promise<Result<any, ApiError>> {
      this.list.loading = true; // Use list loading for now
      this.error = null;
      const result = await this.services.memory.analyzePhoto(command);
      this.list.loading = false;
      return result;
    },

    async generateStory(command: any): Promise<Result<any, ApiError>> {
      this.list.loading = true; // Use list loading for now
      this.error = null;
      const result = await this.services.memory.generateStory(command);
      this.list.loading = false;
      return result;
    },
  },
});