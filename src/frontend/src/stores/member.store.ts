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
      loading: false, // Loading state for the list of members
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
      editableMembers: [] as Member[], // New state for editable members
    },

    // State for single item operations
    detail: {
      item: null as Member | null,
      loading: false, // Loading state for a single member
    },
  }),

  getters: {},

  actions: {
    async loadEditableMembers() {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.loadItems(
        {
          searchQuery: '',
          familyId: undefined,
          gender: undefined,
        },
        1, // page
        5000, // itemsPerPage (a large number to get all editable members)
      );

      if (result.ok) {
        this.list.editableMembers = result.value.items; // Extract items from paginated result
      } else {
        this.error = i18n.global.t('member.errors.load');
        console.error(result.error);
      }
      this.list.loading = false;
    },

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

    async addItem(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('member.errors.add');
        console.error(result.error);
      }
      this.list.loading = false;
      return result;
    },

    async addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.addItems(newItems);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('member.errors.add');
        console.error(result.error);
      }
      this.list.loading = false;
      return result;
    },

    async updateItem(updatedItem: Member): Promise<void> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.update(updatedItem);
      if (result.ok) {
       await this._loadItems();
      } else {
        this.error = i18n.global.t('member.errors.update');
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.member.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || 'Failed to delete member.';
        console.error(result.error);
      }
      this.list.loading = false;
      return result;
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.list.totalPages && this.list.currentPage !== page) {
        this.list.currentPage = page;
        await this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0 && this.list.itemsPerPage !== count) {
        this.list.itemsPerPage = count;
        this.list.currentPage = 1; // Reset to first page when items per page changes
        await this._loadItems();
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.list.sortBy = sortBy;
      this.list.currentPage = 1; // Reset to first page on sort change
      this._loadItems();
    },

    async setCurrentItem(item: Member | null) {
      this.detail.item = item;
    },

    async getById(id: string): Promise<Member | undefined> {
      this.detail.loading = true;
      this.error = null;

      // const cachedMember = this.memberCache.get(id);
      // if (cachedMember) {
      //   this.detail.loading = false;
      //   this.detail.item = cachedMember;
      //   return cachedMember;
      // }

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

    async getByFamilyId(familyId: string): Promise<void> {
      this.list.filters.familyId = familyId;
      this.setPage(1);
      this.setItemsPerPage(5000);
      await this._loadItems();
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
  },
});
