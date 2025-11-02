import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Member, MemberFilter, Result } from '@/types';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';

export const useMemberStore = defineStore('member', {
  state: () => ({
    items: [] as Member[],
    currentItem: null as Member | null,
    loading: false,
    error: null as string | null,
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
  }),

  getters: {},

  actions: {
    async loadEditableMembers() {
      this.loading = true;
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
        this.editableMembers = result.value.items; // Extract items from paginated result
      } else {
        this.error = i18n.global.t('member.errors.load');
        console.error(result.error);
      }
      this.loading = false;
    },

    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.loadItems({
        ...this.filters,
        sortBy: this.sortBy.length > 0 ? this.sortBy[0].key : undefined,
        sortOrder: this.sortBy.length > 0 ? (this.sortBy[0].order as 'asc' | 'desc') : undefined,
      },
        this.currentPage,
        this.itemsPerPage,
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('member.errors.load');
        this.items = [];
        this.totalItems = 0;
        this.totalPages = 1;
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('member.errors.add');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.addItems(newItems);
      if (result.ok) {
        await this._loadItems(); // Re-fetch to update pagination and filters
      } else {
        this.error = i18n.global.t('member.errors.add');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async updateItem(updatedItem: Member): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.update(updatedItem);
      if (result.ok) {
       await this._loadItems();
      } else {
        this.error = i18n.global.t('member.errors.update');
        console.error(result.error);
      }
      this.loading = false;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || 'Failed to delete member.';
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages && this.currentPage !== page) {
        this.currentPage = page;
        await this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0 && this.itemsPerPage !== count) {
        this.itemsPerPage = count;
        this.currentPage = 1; // Reset to first page when items per page changes
        await this._loadItems();
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      this.sortBy = sortBy;
      this.currentPage = 1; // Reset to first page on sort change
      this._loadItems();
    },

    async setCurrentItem(item: Member | null) {
      this.currentItem = item;
    },

    async getById(id: string): Promise<Member | undefined> {
      this.loading = true;
      this.error = null;

      // const cachedMember = this.memberCache.get(id);
      // if (cachedMember) {
      //   this.loading = false;
      //   this.currentItem = cachedMember;
      //   return cachedMember;
      // }

      const result = await this.services.member.getById(id);
      this.loading = false;
      if (result.ok) {
        if (result.value) {
          this.currentItem = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('member.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    async getByFamilyId(familyId: string): Promise<void> {
      this.filters.familyId = familyId;
      this.setPage(1);
      this.setItemsPerPage(5000);
      await this._loadItems();
    },

    async getByIds(ids: string[]): Promise<Member[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.getByIds(ids);
      this.loading = false;
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
      this.items = [];
      this.totalItems = 0;
      this.totalPages = 1;
    },
  },
});
