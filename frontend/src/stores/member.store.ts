import { defineStore } from 'pinia';
import type { Member } from '@/types/family';
import type { MemberFilter } from '@/services/member';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

export const useMemberStore = defineStore('member', {
  state: () => ({
    items: [] as Member[],
    currentItem: null as Member | null,
    loading: false,
    error: null as string | null,
    filters: {
      fullName: '',
      dateOfBirth: null,
      dateOfDeath: null,
      gender: undefined,
      placeOfBirth: '',
      placeOfDeath: '',
      occupation: '',
      familyId: undefined,
    } as MemberFilter,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    totalItems: 0,
    totalPages: 1,
  }),

  getters: {
    paginatedItems: (state): Member[] => {
      return state.items;
    },

    /** Lấy 1 member theo id */
    getItemById: (state) => (id: string) => {
      return state.items.find((m) => m.id === id);
    },

    /** Lấy nhiều member theo id */
    getItemsByIds: (state) => (ids: string[]) => {
      return state.items.filter((m) => ids.includes(m.id));
    },
  },

  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.searchMembers(
        this.filters,
        this.currentPage,
        this.itemsPerPage,
      );

      if (result.ok) {
        this.items = result.value.items;
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách thành viên.';
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
      if (
        newItem.lastName.trim() === '' ||
        newItem.firstName.trim() === ''
      ) {
        this.error = 'Họ và tên không được để trống.';
        this.loading = false;
        return; // Return early
      }
      if (
        newItem.dateOfBirth &&
        newItem.dateOfDeath &&
        newItem.dateOfBirth > newItem.dateOfDeath
      ) {
        this.error = 'Ngày sinh không thể sau ngày mất.';
        this.loading = false;
        return; // Return early
      }
      if (
        newItem.placeOfBirth &&
        newItem.placeOfDeath &&
        newItem.placeOfBirth === newItem.placeOfDeath
      ) {
        this.error = 'Nơi sinh và nơi mất không thể giống nhau.';
        this.loading = false;
        return; // Return early
      }
      if (newItem.occupation && newItem.occupation.length > 100) {
        this.error = 'Nghề nghiệp không được vượt quá 100 ký tự.';
        this.loading = false;
        return; // Return early
      }

      const result = await this.services.member.add(newItem);
      if (result.ok) {
        this.items.push(result.value);
        await this._loadItems();
      } else {
        this.error = result.error.message || 'Không thể thêm thành viên.';
        console.error(result.error);
      }
      this.loading = false;
    },

    async updateItem(updatedItem: Member) {
      this.loading = true;
      this.error = null;
      if (
        updatedItem.lastName.trim() === '' ||
        updatedItem.firstName.trim() === ''
      ) {
        this.error = 'Họ và tên không được để trống.';
        this.loading = false;
        return; // Return early
      }
      if (
        updatedItem.dateOfBirth &&
        updatedItem.dateOfDeath &&
        updatedItem.dateOfBirth > updatedItem.dateOfDeath
      ) {
        this.error = 'Ngày sinh không thể sau ngày mất.';
        this.loading = false;
        return; // Return early
      }
      if (
        updatedItem.placeOfBirth &&
        updatedItem.placeOfDeath &&
        updatedItem.placeOfBirth === updatedItem.placeOfDeath
      ) {
        this.error = 'Nơi sinh và nơi mất không thể giống nhau.';
        this.loading = false;
        return; // Return early
      }
      if (updatedItem.occupation && updatedItem.occupation.length > 100) {
        this.error = 'Nghề nghiệp không được vượt quá 100 ký tự.';
        this.loading = false;
        return; // Return early
      }
      const result = await this.services.member.update(updatedItem);
      if (result.ok) {
        const idx = this.items.findIndex((m) => m.id === result.value.id);
        if (idx !== -1) this.items[idx] = result.value;
      } else {
        this.error = result.error.message || 'Không thể cập nhật thành viên.';
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

    async searchItems(filters: MemberFilter) {
      const newFilters: MemberFilter = { ...filters };
      if (typeof newFilters.dateOfBirth === 'string') {
        newFilters.dateOfBirth = new Date(newFilters.dateOfBirth);
      }
      if (typeof newFilters.dateOfDeath === 'string') {
        newFilters.dateOfDeath = new Date(newFilters.dateOfDeath);
      }
      this.filters = { ...this.filters, ...newFilters };
      this.currentPage = 1;
      await this._loadItems();
    },

    // New searchLookup for Lookup component
    async searchLookup(filters: MemberFilter, page: number, itemsPerPage: number) {
      // Only apply filters relevant to Lookup: searchQuery (from searchTerm) and familyId
      const newFilters: MemberFilter = {
        fullName: filters.searchQuery, // Map searchQuery to fullName for filtering
        familyId: filters.familyId,
      };

      this.filters = { ...this.filters, ...newFilters };
      this.currentPage = page; // Use passed page
      this.itemsPerPage = itemsPerPage; // Use passed itemsPerPage
      await this._loadItems();
    },

    async getManyItemsByIds(ids: string[]): Promise<Member[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.getManyByIds(ids);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = result.error.message || 'Không thể tải danh sách thành viên.';
        console.error(result.error);
        return [];
      }
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        await this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0) {
        this.itemsPerPage = count;
        this.currentPage = 1;
        await this._loadItems();
      }
    },

    setCurrentItem(item: Member | null) {
      this.currentItem = item;
    },
  },
});


