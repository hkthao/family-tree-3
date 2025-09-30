import { defineStore } from 'pinia';
import type { Member, MemberFilter } from '@/types/family/member';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';

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
      const result = await this.services.member.loadItems(
        this.filters,
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

      if (
        !newItem.lastName ||
        !newItem.firstName ||
        newItem.lastName.trim() === '' ||
        newItem.firstName.trim() === ''
      ) {
        this.error = i18n.global.t('member.errors.emptyName');
        this.loading = false;
        return;
      }

      if (newItem.dateOfBirth && newItem.dateOfDeath && newItem.dateOfBirth > newItem.dateOfDeath) {
        this.error = i18n.global.t('member.errors.birthAfterDeath');
        this.loading = false;
        return;
      }

      if (
        newItem.placeOfBirth &&
        newItem.placeOfDeath &&
        newItem.placeOfBirth.trim() === newItem.placeOfDeath.trim()
      ) {
        this.error = i18n.global.t('member.errors.sameBirthAndDeathPlace');
        this.loading = false;
        return;
      }

      if (newItem.occupation && newItem.occupation.length > 100) {
        this.error = i18n.global.t('member.errors.occupationTooLong');
        this.loading = false;
        return;
      }

      const result = await this.services.member.add(newItem);
      if (result.ok) {
        this.items.push(result.value);
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

      if (
        !updatedItem.lastName ||
        !updatedItem.firstName ||
        updatedItem.lastName.trim() === '' ||
        updatedItem.firstName.trim() === ''
      ) {
        this.error = i18n.global.t('member.errors.emptyName');
        this.loading = false;
        return;
      }

      if (
        updatedItem.dateOfBirth &&
        updatedItem.dateOfDeath &&
        updatedItem.dateOfBirth > updatedItem.dateOfDeath
      ) {
        this.error = i18n.global.t('member.errors.birthAfterDeath');
        this.loading = false;
        return;
      }

      if (
        updatedItem.placeOfBirth &&
        updatedItem.placeOfDeath &&
        updatedItem.placeOfBirth.trim() === updatedItem.placeOfDeath.trim()
      ) {
        this.error = i18n.global.t('member.errors.sameBirthAndDeathPlace');
        this.loading = false;
        return;
      }

      if (updatedItem.occupation && updatedItem.occupation.length > 100) {
        this.error = i18n.global.t('member.errors.occupationTooLong');
        this.loading = false;
        return;
      }
      const result = await this.services.member.update(updatedItem);
      if (result.ok) {
        const idx = this.items.findIndex((m) => m.id === result.value.id);
        if (idx !== -1) this.items[idx] = result.value;
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

    async loadItems(filters: MemberFilter) {
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
      const result = await this.services.member.getByIds(ids);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = i18n.global.t('member.errors.load');
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

    async setCurrentItem(item: Member | null) {
      this.currentItem = item;
    },

    async getItemById(id: string): Promise<Member | undefined> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.getById(id);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = i18n.global.t('member.errors.loadById');
        console.error(result.error);
        return undefined;
      }
    },

    async getMembersByFamilyId(familyId: string): Promise<Member[]> {
      this.loading = true;
      this.error = null;
      const result = await this.services.member.fetchMembersByFamilyId(familyId);
      this.loading = false;
      if (result.ok) {
        return result.value;
      } else {
        this.error = i18n.global.t('member.errors.loadByFamilyId');
        console.error(result.error);
        return [];
      }
    }
  },
});


