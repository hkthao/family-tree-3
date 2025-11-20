import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Member, MemberFilter, Result } from '@/types';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';

export const useMemberStore = defineStore('member', {
  state: () => ({
    // General state
    error: null as string | null,

    // State for list operations
    list: {
      items: [] as Member[],
      loading: false, // Loading state for the list of members (e.g., _loadItems, loadEditableMembers, getByIds)
      filters: {
        searchQuery: '',
        familyId: undefined,
        gender: undefined,
      } as MemberFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[], // Sorting key and order
    },

    // State for single item operations
    detail: {
      item: null as Member | null,
      loading: false, // Loading state for a single member
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
      const result = await this.services.member.loadItems({
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
        this.list.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('member.errors.load');
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: Member): Promise<Result<Member, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.member.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('member.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.member.addItems(newItems);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('member.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: Member): Promise<Result<Member, ApiError>> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.member.update(updatedItem);
      if (result.ok) {
       await this._loadItems();
      } else {
        this.error = i18n.global.t('member.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
      return result; // Return the result
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.member.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || 'Failed to delete member.';
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
        this.list.currentPage = 1; // Reset to first page when items per page changes
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.list.sortBy = sortBy;
      this.list.currentPage = 1; // Reset to first page on sort change
    },

    setListOptions(options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) {
      // Cập nhật trang hiện tại nếu nó thay đổi
      if (this.list.currentPage !== options.page) {
        this.list.currentPage = options.page;
      }

      // Cập nhật số lượng mục trên mỗi trang nếu nó thay đổi
      if (this.list.itemsPerPage !== options.itemsPerPage) {
        this.list.itemsPerPage = options.itemsPerPage;
      }

      // Cập nhật sắp xếp nếu nó thay đổi
      // So sánh mảng sortBy để tránh cập nhật không cần thiết
      const currentSortBy = JSON.stringify(this.list.sortBy);
      const newSortBy = JSON.stringify(options.sortBy);
      if (currentSortBy !== newSortBy) {
        this.list.sortBy = options.sortBy;
      }

      // Sau khi tất cả các tùy chọn đã được cập nhật, gọi _loadItems một lần duy nhất
      this._loadItems();
    },

    async setCurrentItem(item: Member | null) {
      this.detail.item = item;
    },

    async getById(id: string): Promise<Member | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.member.getById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('member.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    async getByIds(ids: string[]): Promise<Member[]> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.getByIds(ids);
      this.list.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error =
          result.error.message || 'Không thể tải danh sách thành viên.';
        console.error(result.error);
        return [];
      }
    },

    clearItems() {
      this.list.items = [];
      this.list.totalItems = 0;
      this.list.totalPages = 1;
    },

    async searchMembers(searchQuery: string) {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.loadItems({
        searchQuery,
      }, 1, 10); // Search top 10

      if (result.ok) {
        this.list.items = result.value.items;
      } else {
        this.error = i18n.global.t('member.errors.load');
        this.list.items = [];
        console.error(result.error);
      }
      this.list.loading = false;
    },
  },
});
