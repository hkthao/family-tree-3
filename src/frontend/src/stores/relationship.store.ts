import { defineStore } from 'pinia';
import type { Relationship, Result, RelationshipFilter } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import { IdCache } from '@/utils/cacheUtils'; // Import IdCache

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
    relationshipCache: new IdCache<Relationship>(), // Add relationshipCache to state
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
        this.relationshipCache.setMany(result.value.items); // Cache fetched items
      } else {
        this.error = i18n.global.t('relationship.errors.load');
        console.error(result.error);
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<Relationship, 'id'>): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.add(newItem);
      if (result.ok) {
        this.relationshipCache.clear(); // Invalidate cache on add
        await this._loadItems();
      } else {
        this.error = i18n.global.t('relationship.errors.add');
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: Relationship): Promise<void> {
      this.loading = true;
      this.error = null;
      const result = await this.services.relationship.update(updatedItem);
      if (result.ok) {
        this.relationshipCache.clear(); // Invalidate cache on update
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
        this.relationshipCache.clear(); // Invalidate cache on delete
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

    async getById(id: string): Promise<Relationship | undefined> {
      this.loading = true;
      this.error = null;

      const cachedRelationship = this.relationshipCache.get(id);
      if (cachedRelationship) {
        this.loading = false;
        this.currentItem = cachedRelationship;
        return cachedRelationship;
      }

      const result = await this.services.relationship.getById(id);
      this.loading = false;
      if (result.ok) {
        if (result.value) {
          this.currentItem = result.value;
          this.relationshipCache.set(result.value); // Cache the fetched relationship
          return result.value;
        }
      } else {
        this.error = i18n.global.t('relationship.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    async getByIds(ids: string[]): Promise<Relationship[]> {
      this.loading = true;
      this.error = null;

      const result = await this.relationshipCache.getMany(ids, (missingIds) =>
        this.services.relationship.getByIds(missingIds),
      );

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

    async addItems(newItems: Omit<Relationship, 'id'>[]): Promise<Result<string[], ApiError>> { // Changed return type
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.relationship.addItems(newItems); // Changed

        if (result.ok) {
          this.relationshipCache.clear(); // Invalidate cache on add
          await this._loadItems();
          return result; // Return the result from the service
        } else {
          this.error = i18n.global.t('relationship.errors.add');
          console.error(result.error);
          return result; // Return the error result
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('relationship.errors.add');
        console.error(err);
        return { ok: false, error: { message: this.error } } as Result<string[], ApiError>; // Return a failure result
      } finally {
        this.loading = false;
      }
    },
  },
});
