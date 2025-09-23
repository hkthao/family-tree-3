import { defineStore } from 'pinia';
import type { Member } from '@/types/member';
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
  }),

  getters: {
    /** Lọc dữ liệu dựa trên filters */
    filteredItems: (state): Member[] => {
      const f = state.filters;
      return state.items
        ? state.items.filter((m: Member) => {
            if (f.fullName) {
              const lowerCaseFullName = f.fullName.toLowerCase();
              if (
                !(
                  m.fullName &&
                  m.fullName.toLowerCase().includes(lowerCaseFullName)
                )
              ) {
                return false;
              }
            }
            // Compare Date objects
            if (
              f.dateOfBirth &&
              m.dateOfBirth &&
              m.dateOfBirth.toISOString().split('T')[0] !==
                f.dateOfBirth.toISOString().split('T')[0]
            )
              return false;
            if (
              f.dateOfDeath &&
              m.dateOfDeath &&
              m.dateOfDeath.toISOString().split('T')[0] !==
                f.dateOfDeath.toISOString().split('T')[0]
            )
              return false;
            if (f.gender && m.gender !== f.gender) return false;
            if (
              f.placeOfBirth &&
              !m.placeOfBirth
                ?.toLowerCase()
                .includes(f.placeOfBirth.toLowerCase())
            )
              return false;
            if (
              f.placeOfDeath &&
              !m.placeOfDeath
                ?.toLowerCase()
                .includes(f.placeOfDeath.toLowerCase())
            )
              return false;
            if (
              f.occupation &&
              !m.occupation?.toLowerCase().includes(f.occupation.toLowerCase())
            )
              return false;
            if (f.familyId && m.familyId !== f.familyId) return false;
            return true;
          })
        : [];
    },

    /** Phân trang dựa trên filteredItems */
    paginatedItems: (state): Member[] => {
      const start = (state.currentPage - 1) * state.itemsPerPage;
      const end = start + state.itemsPerPage;
      return (useMemberStore().filteredItems as Member[]).slice(start, end);
    },

    /** Tổng số trang (không cần lưu state) */
    totalPages: (state): number => {
      const total = (useMemberStore().filteredItems as Member[]).length;
      return Math.ceil(total / state.itemsPerPage);
    },

    /** Lấy 1 member theo id */
    getItemById: (state) => (id: string) => {
      return state.items.find((m) => m.id === id);
    },
  },

  actions: {
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
        this.items = this.items.filter((m) => m.id !== id);
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        }
      } catch (e) {
        this.error =
          e instanceof Error ? e.message : 'Failed to delete member.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    searchItems(filters: MemberFilter) {
      const newFilters: MemberFilter = { ...filters };
      if (typeof newFilters.dateOfBirth === 'string') {
        newFilters.dateOfBirth = new Date(newFilters.dateOfBirth);
      }
      if (typeof newFilters.dateOfDeath === 'string') {
        newFilters.dateOfDeath = new Date(newFilters.dateOfDeath);
      }
      this.filters = { ...this.filters, ...newFilters };
      this.currentPage = 1;
    },

    setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
      }
    },

    setItemsPerPage(count: number) {
      if (count > 0) {
        this.itemsPerPage = count;
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        } else if (this.totalPages === 0) {
          this.currentPage = 1;
        }
      }
    },

    setCurrentItem(item: Member | null) {
      this.currentItem = item;
    },
  },
});
