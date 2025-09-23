import { defineStore } from 'pinia';
import type { Family } from '@/types/family';
import type { Paginated } from '@/types/pagination'; // Correct placement of import

export const useFamilyStore = defineStore('family', {
  state: () => ({
    families: [] as Family[],
    currentFamily: null as Family | null,
    loading: false,
    error: null as string | null,
    searchTerm: '',
    visibilityFilter: 'all' as 'all' | 'public' | 'private',
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: 5, // Default items per page
    totalPages: 0,
  }),
  getters: {
    getFamilyById: (state) => (id: string) => {
      return state.families.find((family) => family.id === id);
    },
    paginatedFamilies: (state) => {
      return state.families; // Families are already paginated by the service
    },
  },
  actions: {
    async _loadFamilies() { // Renamed from fetchFamilies
      this.loading = true;
      this.error = null;
      try {
       // Use the injected service to search with current state parameters
          const response: Paginated<Family> = await this.services.family.searchFamilies(
            this.searchTerm,
            this.visibilityFilter,
            this.currentPage,
            this.itemsPerPage
          );
          this.families = response.items;
          this.totalItems = response.totalItems;
          this.totalPages = response.totalPages;
      } catch (e) {
        this.error = 'Không thể tải danh sách gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addFamily(newFamily: Omit<Family, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
        // Use the injected service
        const addedFamily = await this.services.family.add(newFamily); // Renamed to add
        await this._loadFamilies(); // Re-fetch to update pagination and filters
      } catch (e) {
        this.error = 'Không thể thêm gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateFamily(updatedFamily: Family) {
      this.loading = true;
      this.error = null;
      try {
        // Use the injected service
        const updated = await this.services.family.update(updatedFamily); // Renamed to update
        const index = this.families.findIndex((f) => f.id === updated.id);
        if (index !== -1) {
          this.families[index] = updated;
          await this._loadFamilies(); // Re-fetch to update pagination and filters
        } else {
          throw new Error('Không tìm thấy gia đình để cập nhật trong kho.');
        }
      } catch (e) {
        this.error = 'Không thể cập nhật gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteFamily(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await this.services.family.delete(id); // Renamed to delete
        await this._loadFamilies(); // Re-fetch to update pagination and filters
      } catch (e) {
        this.error = 'Không thể xóa gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async searchFamilies(term: string, visibility: 'all' | 'public' | 'private') {
      this.searchTerm = term;
      this.visibilityFilter = visibility;
      this.currentPage = 1; // Reset to first page on new search
      await this._loadFamilies(); // Trigger fetch with new search terms
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        await this._loadFamilies(); // Trigger fetch with new page
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0) {
        this.itemsPerPage = count;
        this.currentPage = 1; // Reset to first page when items per page changes
        await this._loadFamilies(); // Trigger fetch with new items per page
      }
    },

    setCurrentFamily(family: Family | null) {
      this.currentFamily = family;
    },
  },
});