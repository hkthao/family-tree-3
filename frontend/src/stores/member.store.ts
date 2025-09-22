import { defineStore } from 'pinia';
import type { Member } from '@/types/member';
import type { IMemberService } from '@/services/member/member.service.interface';

declare module 'pinia' {
  export interface PiniaCustomProperties {
    services: {
      member: IMemberService;
    };
  }
}

export const useMemberStore = defineStore('member', {
  state: () => ({
    members: [] as Member[],
    currentMember: null as Member | null,
    loading: false,
    error: null as string | null,
    searchTerm: '',
    filteredMembers: [] as Member[],
    currentPage: 1,
    itemsPerPage: 10, // Default items per page
    totalPages: 0,
  }),
  getters: {
    getMemberById: (state) => (id: string) => {
      return state.members.find((member) => member.id === id);
    },
    getFilteredMembers: (state) => {
      if (!state.searchTerm) {
        return state.members;
      }
      const lowerCaseSearchTerm = state.searchTerm.toLowerCase();
      return state.members.filter(
        (member) =>
          member.fullName.toLowerCase().includes(lowerCaseSearchTerm) || // Changed from name
          (member.placeOfBirth && member.placeOfBirth.toLowerCase().includes(lowerCaseSearchTerm)) // Changed from address
      );
    },
    paginatedMembers: (state) => {
      const start = (state.currentPage - 1) * state.itemsPerPage;
      const end = start + state.itemsPerPage;
      return state.filteredMembers.slice(start, end);
    },
  },
  actions: {
    async fetchMembers(familyId?: string) {
      this.loading = true;
      this.error = null;
      try {
        let fetchedMembers: Member[];
        if (familyId) {
          fetchedMembers = await this.services.member.fetchMembersByFamilyId(familyId);
        } else {
          fetchedMembers = await this.services.member.fetchMembers();
        }
        this.members = fetchedMembers;
        this.filteredMembers = this.getFilteredMembers; // Re-filter based on new data
        this.totalPages = Math.ceil(this.filteredMembers.length / this.itemsPerPage);
      } catch (e) {
        this.error = 'Failed to fetch members.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async addMember(newMember: Omit<Member, 'id'>) {
      this.loading = true;
      this.error = null;
      try {
        const addedMember = await this.services.member.addMember(newMember);
        this.members.push(addedMember);
        this.filteredMembers = this.getFilteredMembers; // Update filtered list
        this.totalPages = Math.ceil(this.filteredMembers.length / this.itemsPerPage);
      } catch (e) {
        this.error = 'Failed to add member.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    async updateMember(updatedMember: Member) {
      this.loading = true;
      this.error = null;
      try {
        const updated = await this.services.member.updateMember(updatedMember);
        const index = this.members.findIndex((m) => m.id === updated.id);
        if (index !== -1) {
          this.members[index] = updated;
          this.filteredMembers = this.getFilteredMembers; // Update filtered list
          this.totalPages = Math.ceil(this.filteredMembers.length / this.itemsPerPage);
        } else {
          throw new Error('Member not found for update in store.');
        }
      } catch (e) {
        this.error = 'Failed to update member.';
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
        this.filteredMembers = this.getFilteredMembers; // Update filtered list
        this.totalPages = Math.ceil(this.filteredMembers.length / this.itemsPerPage);
        if (this.currentPage > this.totalPages && this.totalPages > 0) {
          this.currentPage = this.totalPages;
        } else if (this.totalPages === 0) {
          this.currentPage = 1;
        }
      } catch (e) {
        this.error = 'Failed to delete member.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    searchMembers(term: string) {
      this.searchTerm = term;
      this.filteredMembers = this.getFilteredMembers; // Re-filter based on new term
      this.totalPages = Math.ceil(this.filteredMembers.length / this.itemsPerPage);
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
        this.totalPages = Math.ceil(this.filteredMembers.length / this.itemsPerPage);
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
