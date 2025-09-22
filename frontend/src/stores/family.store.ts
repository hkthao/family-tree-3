import { defineStore } from 'pinia';
import type { Family } from '@/types/family';
import type { IFamilyService } from '@/services'; // Import the interface

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: {
      family: IFamilyService;
    };
  }
}

export const useFamilyStore = defineStore('family', {
  state: () => ({
    families: [] as Family[],
    currentFamily: null as Family | null,
    loading: false,
    error: null as string | null,
    searchTerm: '',
    filteredFamilies: [] as Family[],
    currentPage: 1,
    itemsPerPage: 5, // Default items per page
    totalPages: 0,
  }),
  getters: {
    getFamilyById: (state) => (id: string) => {
      return state.families.find((family) => family.id === id);
    },
    getFilteredFamilies: (state) => {
      if (!state.searchTerm) {
        return state.families;
      }
      const lowerCaseSearchTerm = state.searchTerm.toLowerCase();
      return state.families.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseSearchTerm) ||
          (family.description && family.description.toLowerCase().includes(lowerCaseSearchTerm))
      );
    },
    paginatedFamilies: (state) => {
      const start = (state.currentPage - 1) * state.itemsPerPage;
      const end = start + state.itemsPerPage;
      return state.filteredFamilies.slice(start, end);
    },
  },
  actions: {
    async fetchFamilies() {
      this.loading = true;
      this.error = null;
      try {
        // Use the injected service
        const fetchedFamilies = await this.services.family.fetchFamilies();
        this.families = fetchedFamilies;
        this.filteredFamilies = this.getFilteredFamilies; // Re-filter based on new data
        this.totalPages = Math.ceil(this.filteredFamilies.length / this.itemsPerPage);
      } catch (e) {
        this.error = 'Failed to fetch families.';
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
        const addedFamily = await this.services.family.addFamily(newFamily);
        this.families.push(addedFamily);
        this.filteredFamilies = this.getFilteredFamilies; // Update filtered list
        this.totalPages = Math.ceil(this.filteredFamilies.length / this.itemsPerPage);
      } catch (e) {
        this.error = 'Failed to add family.';
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
        const updated = await this.services.family.updateFamily(updatedFamily);
        const index = this.families.findIndex((f) => f.id === updated.id);
        if (index !== -1) {
          this.families[index] = updated;
          this.filteredFamilies = this.getFilteredFamilies; // Update filtered list
          this.totalPages = Math.ceil(this.filteredFamilies.length / this.itemsPerPage);
        } else {
          throw new Error('Family not found for update in store.');
        }
      } catch (e) {
        this.error = 'Failed to update family.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteFamily(id: string) {
      this.loading = true;
      this.error = null;
      try {
        // Use the injected service
        await this.services.family.deleteFamily(id);
        this.families = this.families.filter((f) => f.id !== id);
        this.filteredFamilies = this.getFilteredFamilies; // Update filtered list
        this.totalPages = Math.ceil(this.filteredFamilies.length / this.itemsPerPage);
        // Adjust current page if it's out of bounds after deletion
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        } else if (this.totalPages === 0) {
          this.currentPage = 1;
        }
      } catch (e) {
        this.error = 'Failed to delete family.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    searchFamilies(term: string) {
      this.searchTerm = term;
      this.filteredFamilies = this.getFilteredFamilies; // Re-filter based on new term
      this.totalPages = Math.ceil(this.filteredFamilies.length / this.itemsPerPage);
      this.currentPage = 1; // Reset to first page on new search
    },

    setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
      }
    },

    setItemsPerPage(count: number) {
      if (count > 0) {
        this.itemsPerPage = count;
        this.totalPages = Math.ceil(this.filteredFamilies.length / this.itemsPerPage);
        // Adjust current page if it's out of bounds after changing items per page
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        } else if (this.totalPages === 0) {
          this.currentPage = 1;
        }
      }
    },

    setCurrentFamily(family: Family | null) {
      this.currentFamily = family;
    },
  },
});