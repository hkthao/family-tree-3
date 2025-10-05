import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { Member, MemberFilter } from '@/types';
import { defineStore } from 'pinia';

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
  }),

  getters: {},

  actions: {
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

    async addItem(newItem: Omit<Member, 'id'>) {
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
    },

    async updateItem(updatedItem: Member) {
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

    async deleteItem(id: string) {
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

    async getById(id: string): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.getById(id);
      this.loading = false;
      if (result.ok) {
        this.currentItem = { ...(result.value as Member) };
      } else {
        this.error = i18n.global.t('member.errors.loadById');
        console.error(result.error);
      }
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
  },
});
