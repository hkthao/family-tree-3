import { defineStore } from 'pinia';
import type { Member } from '@/types/member';
import type { MemberFilter } from '@/services/member';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import type { Paginated } from '@/types/pagination';

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
  },

  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      try {
        const response: Paginated<Member> =
          await this.services.member.searchMembers(
            this.filters,
            this.currentPage,
            this.itemsPerPage,
          );
        this.items = response.items;
        this.totalItems = response.totalItems;
        this.totalPages = response.totalPages;
      } catch (e) {
        this.error = 'Không thể tải danh sách thành viên.';
        this.items = [];
        this.totalItems = 0;
        this.totalPages = 1;
        console.error(e);
      } finally {
        this.loading = false;
      }
    },
    async fetchItems(familyId?: string) {
      this.loading = true;
      this.error = null;
      try {
        this.items = familyId
          ? await this.services.member.fetchMembersByFamilyId(familyId)
          : await this.services.member.fetch(); // Renamed to fetch and added fullName
        this.currentPage = 1;
      } catch (e) {
        this.error =
          e instanceof Error
            ? e.message
            : 'Không thể tải danh sách thành viên.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addItem(newItem: Omit<Member, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
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

        const added = await this.services.member.add(newItem); // Renamed to add
        this.items.push(added);
        await this._loadItems();
      } catch (e) {
        this.error =
          e instanceof Error ? e.message : 'Không thể thêm thành viên.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateItem(updatedItem: Member) {
      this.loading = true;
      this.error = null;
      try {
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
        const updated = await this.services.member.update(updatedItem); // Renamed to update
        const idx = this.items.findIndex((m) => m.id === updated.id);
        if (idx !== -1) this.items[idx] = updated;
      } catch (e) {
        this.error =
          e instanceof Error ? e.message : 'Không thể cập nhật thành viên.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await this.services.member.delete(id); // Renamed to delete
        await this._loadItems();
      } catch (e) {
        this.error =
          e instanceof Error ? e.message : 'Failed to delete member.';
        console.error(e);
      } finally {
        this.loading = false;
      }
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
