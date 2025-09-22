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
      return state.members.filter((m: Member) => {
        if (f.fullName && !m.fullName.toLowerCase().includes(f.fullName.toLowerCase())) return false;
        if (f.dateOfBirth && m.dateOfBirth !== f.dateOfBirth) return false;
        if (f.dateOfDeath && m.dateOfDeath !== f.dateOfDeath) return false;
        if (f.gender && m.gender !== f.gender) return false;
        if (f.placeOfBirth && (!m.placeOfBirth?.toLowerCase().includes(f.placeOfBirth.toLowerCase()))) return false;
        if (f.placeOfDeath && (!m.placeOfDeath?.toLowerCase().includes(f.placeOfDeath.toLowerCase()))) return false;
        if (f.occupation && (!m.occupation?.toLowerCase().includes(f.occupation.toLowerCase()))) return false;
        if (f.familyId && m.familyId !== f.familyId) return false;
        return true;
      });
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
        this.members = familyId
          ? await this.services.member.fetchMembersByFamilyId(familyId)
          : await this.services.member.fetchMembers();
        this.currentPage = 1;
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Failed to fetch members.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addMember(newMember: Omit<Member, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
        if(newMember.fullName.trim() === '') {
          throw new Error('Full name cannot be empty.');
        }
        if(newMember.dateOfBirth && newMember.dateOfDeath && new Date(newMember.dateOfBirth) > new Date(newMember.dateOfDeath)) {
          throw new Error('Date of birth cannot be later than date of death.');
        }
        if(newMember.placeOfBirth && newMember.placeOfDeath && newMember.placeOfBirth === newMember.placeOfDeath) {
          throw new Error('Place of birth and place of death cannot be the same.');
        }
        if(newMember.occupation && newMember.occupation.length > 100) {
          throw new Error('Occupation cannot exceed 100 characters.');
        }
        
        const added = await this.services.member.addMember(newMember);
        this.members.push(added);
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Failed to add member.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateMember(updatedMember: Member) {
      this.loading = true;
      this.error = null;
      try {
        if(updatedMember.fullName.trim() === '') {
          throw new Error('Full name cannot be empty.');
        }
        if(updatedMember.dateOfBirth && updatedMember.dateOfDeath && new Date(updatedMember.dateOfBirth) > new Date(updatedMember.dateOfDeath)) {
          throw new Error('Date of birth cannot be later than date of death.');
        }
        if(updatedMember.placeOfBirth && updatedMember.placeOfDeath && updatedMember.placeOfBirth === updatedMember.placeOfDeath) {
          throw new Error('Place of birth and place of death cannot be the same.');
        }
        if(updatedMember.occupation && updatedMember.occupation.length > 100) {
          throw new Error('Occupation cannot exceed 100 characters.');
        }
        const updated = await this.services.member.updateMember(updatedMember);
        const idx = this.members.findIndex((m) => m.id === updated.id);
        if (idx !== -1) this.members[idx] = updated;
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Failed to update member.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async deleteMember(id: string) {
      this.loading = true;
      this.error = null;
      try {
        await this.services.member.deleteMember(id);
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
      this.filters = { ...this.filters, ...filters };
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
