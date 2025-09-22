import { defineStore } from 'pinia';
import type { Member } from '@/types/member';
import type { MemberFilter } from '@/services/member';

export const useMemberStore = defineStore('member', {
  state: () => ({
    members: [] as Member[],
    currentMember: null as Member | null,
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
    itemsPerPage: 10,
  }),

  getters: {
    /** Lọc dữ liệu dựa trên filters */
    filteredMembers: (state): Member[] => {
      const f = state.filters;
      return state.members ? state.members.filter((m: Member) => {
        if (f.fullName) {
          const lowerCaseFullName = f.fullName.toLowerCase();
          if (!(m.fullName && m.fullName.toLowerCase().includes(lowerCaseFullName))) {
            return false;
          }
        }
        // Compare Date objects
        if (f.dateOfBirth && m.dateOfBirth && m.dateOfBirth.toISOString().split('T')[0] !== f.dateOfBirth.toISOString().split('T')[0]) return false;
        if (f.dateOfDeath && m.dateOfDeath && m.dateOfDeath.toISOString().split('T')[0] !== f.dateOfDeath.toISOString().split('T')[0]) return false;
        if (f.gender && m.gender !== f.gender) return false;
        if (f.placeOfBirth && (!m.placeOfBirth?.toLowerCase().includes(f.placeOfBirth.toLowerCase()))) return false;
        if (f.placeOfDeath && (!m.placeOfDeath?.toLowerCase().includes(f.placeOfDeath.toLowerCase()))) return false;
        if (f.occupation && (!m.occupation?.toLowerCase().includes(f.occupation.toLowerCase()))) return false;
        if (f.familyId && m.familyId !== f.familyId) return false;
        return true;
      }) : [];
    },

    /** Phân trang dựa trên filteredMembers */
    paginatedMembers: (state): Member[] => {
      const start = (state.currentPage - 1) * state.itemsPerPage;
      const end = start + state.itemsPerPage;
      return (useMemberStore().filteredMembers as Member[]).slice(start, end);
    },

    /** Tổng số trang (không cần lưu state) */
    totalPages: (state): number => {
      const total = (useMemberStore().filteredMembers as Member[]).length;
      return Math.ceil(total / state.itemsPerPage);
    },

    /** Lấy 1 member theo id */
    getMemberById: (state) => (id: string) => {
      return state.members.find((m) => m.id === id);
    },

  },

  actions: {
    async fetchMembers(familyId?: string) {
      this.loading = true;
      this.error = null;
      try {
       // this.members = familyId
      //    ? (await this.services.member.fetchMembersByFamilyId(familyId)).map(m => ({ ...m, fullName: `${m.firstName} ${m.lastName}`.trim() }))
       //   : (await this.services.member.fetch()).map(m => ({ ...m, fullName: `${m.firstName} ${m.lastName}`.trim() })); // Renamed to fetch and added fullName
        this.currentPage = 1;
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Không thể tải danh sách thành viên.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addMember(newMember: Omit<Member, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
        if(newMember.lastName.trim() === '' || newMember.firstName.trim() === '') {
          this.error = 'Họ và tên không được để trống.';
          this.loading = false;
          return; // Return early
        }
        if(newMember.dateOfBirth && newMember.dateOfDeath && newMember.dateOfBirth > newMember.dateOfDeath) {
          this.error = 'Ngày sinh không thể sau ngày mất.';
          this.loading = false;
          return; // Return early
        }
        if(newMember.placeOfBirth && newMember.placeOfDeath && newMember.placeOfBirth === newMember.placeOfDeath) {
          this.error = 'Nơi sinh và nơi mất không thể giống nhau.';
          this.loading = false;
          return; // Return early
        }
        if(newMember.occupation && newMember.occupation.length > 100) {
          this.error = 'Nghề nghiệp không được vượt quá 100 ký tự.';
          this.loading = false;
          return; // Return early
        }
        
        const added = await this.services.member.add(newMember); // Renamed to add
        this.members.push(added);
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Không thể thêm thành viên.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateMember(updatedMember: Member) {
      this.loading = true;
      this.error = null;
      try {
        if(updatedMember.lastName.trim() === '' || updatedMember.firstName.trim() === '') {
          this.error = 'Họ và tên không được để trống.';
          this.loading = false;
          return; // Return early
        }
        if(updatedMember.dateOfBirth && updatedMember.dateOfDeath && updatedMember.dateOfBirth > updatedMember.dateOfDeath) {
          this.error = 'Ngày sinh không thể sau ngày mất.';
          this.loading = false;
          return; // Return early
        }
        if(updatedMember.placeOfBirth && updatedMember.placeOfDeath && updatedMember.placeOfBirth === updatedMember.placeOfDeath) {
          this.error = 'Nơi sinh và nơi mất không thể giống nhau.';
          this.loading = false;
          return; // Return early
        }
        if(updatedMember.occupation && updatedMember.occupation.length > 100) {
          this.error = 'Nghề nghiệp không được vượt quá 100 ký tự.';
          this.loading = false;
          return; // Return early
        }
        const updated = await this.services.member.update(updatedMember); // Renamed to update
        const idx = this.members.findIndex((m) => m.id === updated.id);
        if (idx !== -1) this.members[idx] = updated;
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Không thể cập nhật thành viên.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteMember(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await this.services.member.delete(id); // Renamed to delete
        this.members = this.members.filter((m) => m.id !== id);
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        }
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Failed to delete member.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    searchMembers(filters: MemberFilter) {
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

    setCurrentMember(member: Member | null) {
      this.currentMember = member;
    },
  },
});
