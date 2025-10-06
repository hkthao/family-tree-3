import { defineStore } from 'pinia';
import type { Relationship, Result, RelationshipFilter } from '@/types';
import type { ApiError } from '@/utils/api';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';

export const useRelationshipStore = defineStore('relationship', {
  state: () => ({
    items: [] as Relationship[],
    currentItem: null as unknown as Relationship,
    loading: false,
    error: null as string | null,
    filter: {} as RelationshipFilter,
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    totalPages: 0,
  }),
  getters: {},
  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.loadItems(
        this.filter,
        this.currentPage,
        this.itemsPerPage,
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('relationship.errors.load');
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<Relationship, 'id'>) {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('relationship.errors.add');
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: Relationship) {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.update(updatedItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('relationship.errors.update');
        console.error(result.error);
      }
      this.loading = false;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('relationship.errors.delete');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages && this.currentPage !== page) {
        this.currentPage = page;
        this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0 && this.itemsPerPage !== count) {
        this.itemsPerPage = count;
        this.currentPage = 1; // Reset to first page when items per page changes
        this._loadItems();
      }
    },

    setSortBy(sortBy: { key: string; order: string }[]) {
      // Assuming RelationshipFilter has sortBy and sortOrder properties
      this.filter.sortBy = sortBy.length > 0 ? sortBy[0].key : undefined;
      this.filter.sortOrder =
        sortBy.length > 0 ? (sortBy[0].order as 'asc' | 'desc') : undefined;
      this.currentPage = 1; // Reset to first page on sort change
      this._loadItems();
    },

    setCurrentItem(item: Relationship) {
      this.currentItem = item;
    },

    async getById(id: string): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.getById(id);
      this.loading = false;
      if (result.ok) {
        this.currentItem = { ...(result.value as Relationship) }; // Set currentItem
      } else {
        this.error = i18n.global.t('relationship.errors.loadById');
        console.error(result.error);
      }
    },

    async getByIds(ids: string[]): Promise<Relationship[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.getByIds(ids);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách quan hệ.';
        console.error(result.error);
        return [];
      }
    },

    async getByFamilyId(familyId: string): Promise<void> {
      this.filter.familyId = familyId;
      this.setPage(1);
      this.setItemsPerPage(5000); // Fetch all relationships for the tree
      await this._loadItems();
    },
  },
});
