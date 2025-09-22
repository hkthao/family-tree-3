import { defineStore } from 'pinia';
import type { FamilyEvent } from '@/types/family-event';
import type { Paginated } from '@/types/pagination';
import { createServices, type ServiceMode } from '@/services/service.factory';

const isMockApi = import.meta.env.VITE_APP_USE_MOCK_API === 'true';
const mode: ServiceMode = isMockApi ? 'mock' : 'real';
const services = createServices(mode);

export const useFamilyEventStore = defineStore('familyEvent', {
  state: () => ({
    familyEvents: [] as FamilyEvent[],
    currentFamilyEvent: null as FamilyEvent | null,
    loading: false,
    error: null as string | null,
    searchTerm: '',
    familyIdFilter: undefined as string | undefined,
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: 10, // Default items per page
    totalPages: 0,
  }),
  getters: {
    getFamilyEventById: (state) => (id: string) => {
      return state.familyEvents.find((event) => event.id === id);
    },
    paginatedFamilyEvents: (state) => {
      return state.familyEvents; // Events are already paginated by the service
    },
  },
  actions: {
    async _loadFamilyEvents() {
      this.loading = true;
      this.error = null;
      try {
        const response: Paginated<FamilyEvent> = await services.familyEvent.searchFamilyEvents(
          this.searchTerm,
          this.familyIdFilter,
          this.currentPage,
          this.itemsPerPage
        );
        this.familyEvents = response.items;
        this.totalItems = response.totalItems;
        this.totalPages = response.totalPages;
      } catch (e) {
        this.error = 'Không thể tải danh sách sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addFamilyEvent(newFamilyEvent: Omit<FamilyEvent, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
        const addedFamilyEvent = await services.familyEvent.add(newFamilyEvent);
        await this._loadFamilyEvents(); // Re-fetch to update pagination and filters
      } catch (e) {
        this.error = 'Không thể thêm sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateFamilyEvent(updatedFamilyEvent: FamilyEvent) {
      this.loading = true;
      this.error = null;
      try {
        const updated = await services.familyEvent.update(updatedFamilyEvent);
        const index = this.familyEvents.findIndex((event) => event.id === updated.id);
        if (index !== -1) {
          this.familyEvents[index] = updated;
          await this._loadFamilyEvents(); // Re-fetch to update pagination and filters
        } else {
          throw new Error('Không tìm thấy sự kiện gia đình để cập nhật trong kho.');
        }
      } catch (e) {
        this.error = 'Không thể cập nhật sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteFamilyEvent(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await services.familyEvent.delete(id);
        await this._loadFamilyEvents(); // Re-fetch to update pagination and filters
      } catch (e) {
        this.error = 'Không thể xóa sự kiện gia đình.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async searchFamilyEvents(term: string, familyId?: string) {
      this.searchTerm = term;
      this.familyIdFilter = familyId;
      this.currentPage = 1; // Reset to first page on new search
      await this._loadFamilyEvents(); // Trigger fetch with new search terms
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        await this._loadFamilyEvents(); // Trigger fetch with new page
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0) {
        this.itemsPerPage = count;
        this.currentPage = 1; // Reset to first page when items per page changes
        await this._loadFamilyEvents(); // Trigger fetch with new items per page
      }
    },

    setCurrentFamilyEvent(event: FamilyEvent | null) {
      this.currentFamilyEvent = event;
    },
  },
});