import { defineStore } from 'pinia';
import type { MemberFace, Paginated, MemberFaceFilter, Result, ApiError } from '@/types';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

export const useMemberFaceStore = defineStore('memberFace', {
  state: () => ({
    list: {
      items: [] as MemberFace[],
      totalItems: 0,
      loading: false,
      filters: {} as MemberFaceFilter,
      options: { page: 1, itemsPerPage: 10, sortBy: [] as { key: string; order: 'asc' | 'desc' }[], sortOrder: '' as 'asc' | 'desc' | '' },
      error: null as ApiError | null,
    },
    detail: {
      item: null as MemberFace | null,
      loading: false,
      error: null as ApiError | null,
    },
    add: {
      loading: false,
      error: null as ApiError | null,
    },
    update: {
      loading: false,
      error: null as ApiError | null,
    },
    delete: {
      loading: false,
      error: null as ApiError | null,
    },
  }),
  actions: {
    setListOptions(options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[]; }) {
      this.list.options.page = options.page;
      this.list.options.itemsPerPage = options.itemsPerPage;
      this.list.options.sortBy = options.sortBy.map(s => ({ key: s.key, order: s.order as 'asc' | 'desc' })); // Explicitly cast order
      this.list.options.sortOrder = this.list.options.sortBy.length > 0 ? this.list.options.sortBy[0].order : '' as 'asc' | 'desc' | '';
    },

    async _loadItems(): Promise<Result<Paginated<MemberFace>, ApiError>> {
      this.list.loading = true;
      this.list.error = null;
      // Access service via this.services
      const result = await this.services.memberFace.search(
        {
          page: this.list.options.page,
          itemsPerPage: this.list.options.itemsPerPage,
          sortBy: this.list.options.sortBy.map(s => ({ key: s.key, order: s.order as 'asc' | 'desc' })),
        },
        this.list.filters
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
      } else {
        this.list.error = result.error;
        const { showSnackbar } = useGlobalSnackbar();
        showSnackbar(result.error.message || 'Error loading member faces', 'error');
      }
      this.list.loading = false;
      return result;
    },

    async getById(id: string): Promise<Result<MemberFace | undefined, ApiError>> {
      this.detail.loading = true;
      this.detail.error = null;
      // Access service via this.services
      const result = await this.services.memberFace.getById(id);

      if (result.ok) {
        this.detail.item = result.value ?? null;
      } else {
        this.detail.error = result.error;
        const { showSnackbar } = useGlobalSnackbar();
        showSnackbar(result.error.message || 'Error fetching member face', 'error');
      }
      this.detail.loading = false;
      return result;
    },

    async addItem(item: Omit<MemberFace, 'id'>): Promise<Result<MemberFace, ApiError>> {
      this.add.loading = true;
      this.add.error = null;
      // Access service via this.services
      const result = await this.services.memberFace.add(item);
      if (!result.ok) {
        this.add.error = result.error;
        const { showSnackbar } = useGlobalSnackbar();
        showSnackbar(result.error.message || 'Error adding member face', 'error');
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(item: MemberFace): Promise<Result<MemberFace, ApiError>> {
      this.update.loading = true;
      this.update.error = null;
      // Access service via this.services
      const result = await this.services.memberFace.update(item);
      if (!result.ok) {
        this.update.error = result.error;
        const { showSnackbar } = useGlobalSnackbar();
        showSnackbar(result.error.message || 'Error updating member face', 'error');
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this.delete.loading = true;
      this.delete.error = null;
      // Access service via this.services
      const result = await this.services.memberFace.delete(id);
      if (!result.ok) {
        this.delete.error = result.error;
        const { showSnackbar } = useGlobalSnackbar();
        showSnackbar(result.error.message || 'Error deleting member face', 'error');
      }
      this.delete.loading = false;
      return result;
    },
  },
});
