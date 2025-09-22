import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types/member';
import type { IMemberService } from '@/services/member/member.service.interface';
import { generateMockMembers, generateMockMember, mockFamilies } from '@/data/mock/member.mock';

// Create a mock service for testing
class MockMemberServiceForTest implements IMemberService {
  private _members: Member[] = generateMockMembers(20); // Start with 20 mock members

  // Getter to return a copy of the members array
  get members(): Member[] {
    return [...this._members]; // Return a shallow copy
  }

  private simulateLatency<T>(data: T, error?: string): Promise<T> {
    return new Promise((resolve, reject) => setTimeout(() => {
      if (error) {
        reject(new Error(error));
      } else {
        resolve(data);
      }
    }, 0));
  }

  async fetchMembers(): Promise<Member[]> {
    return this.simulateLatency(this.members);
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const filtered = this.members.filter(m => m.familyId === familyId);
    return this.simulateLatency(filtered);
  }

  async getMemberById(id: string): Promise<Member | undefined> {
    return this.simulateLatency(this.members.find((m) => m.id === id));
  }

  async addMember(newMember: Omit<Member, 'id'>): Promise<Member> {
    if (!newMember.fullName || newMember.fullName.trim() === '') { // Changed from name
      return this.simulateLatency(undefined as any, 'Name cannot be empty');
    }
    const memberToAdd: Member = { ...newMember, id: generateMockMember().id };
    this._members.push(memberToAdd);
    return this.simulateLatency(memberToAdd);
  }

  async updateMember(updatedMember: Member): Promise<Member> {
    if (!updatedMember.fullName || updatedMember.fullName.trim() === '') { // Changed from name
      return this.simulateLatency(undefined as any, 'Name cannot be empty');
    }
    const index = this._members.findIndex((m) => m.id === updatedMember.id);
    if (index !== -1) {
      this._members[index] = updatedMember;
      return this.simulateLatency(updatedMember);
    }
    throw new Error('Member not found');
  }

  async deleteMember(id: string): Promise<void> {
    const initialLength = this._members.length;
    this._members = this._members.filter((m) => m.id !== id);
    if (this._members.length === initialLength) {
      throw new Error('Member not found');
    }
    return this.simulateLatency(undefined);
  }
}

describe('Member Store', () => {
  let mockMemberService: MockMemberServiceForTest;

  beforeEach(() => {
    mockMemberService = new MockMemberServiceForTest();
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useMemberStore();

    store.services = {
      member: mockMemberService,
    };

    store.$reset();
  });

  it('should have correct initial state', () => {
    const store = useMemberStore();
    expect(store.members).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.searchTerm).toBe('');
    expect(store.filteredMembers).toEqual([]);
    expect(store.currentMember).toBe(null);
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(0);
  });

  it('fetchMembers should populate members array, filteredMembers, and calculate totalPages', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    expect(store.members.length).toBe(20);
    expect(store.loading).toBe(false);
    expect(store.filteredMembers.length).toBe(store.members.length);
    expect(store.totalPages).toBe(2); // 20 members, 10 per page
  });

  it('fetchMembersByFamilyId should filter members correctly', async () => {
    const store = useMemberStore();
    const testFamilyId = mockFamilies[0].id; // Use an ID from the mock families

    // Clear existing members in the mock service for this specific test
    mockMemberService._members = [];

    // Add some members specifically for this family
    mockMemberService._members.push(generateMockMember(testFamilyId));
    mockMemberService._members.push(generateMockMember(testFamilyId));

    await store.fetchMembers(testFamilyId);
    expect(store.members.every(m => m.familyId === testFamilyId)).toBe(true);
    expect(store.members.length).toBe(2);
  });

  it('getMemberById should return the correct member', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const member = store.getMemberById(mockMemberService.members[0].id);
    expect(member).toBeDefined();
    expect(member?.fullName).toBe(mockMemberService.members[0].fullName);
  });

  it('addMember should add a new member and update totalPages', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const initialCount = store.members.length;
    const newMemberData: Omit<Member, 'id'> = { fullName: 'New Member', familyId: mockFamilies[0].id }; // Changed from name
    await store.addMember(newMemberData);
    expect(store.members.length).toBe(initialCount + 1);
    expect(store.members.some(m => m.fullName === 'New Member')).toBe(true);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(Math.ceil((initialCount + 1) / store.itemsPerPage));
  });

  it('addMember should handle empty name correctly', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const initialCount = store.members.length;
    const newMemberData: Omit<Member, 'id'> = { fullName: '', familyId: mockFamilies[0].id }; // Changed from name
    await store.addMember(newMemberData);

    expect(store.members.length).toBe(initialCount);
    expect(store.error).toBe('Failed to add member.');
    expect(store.loading).toBe(false);
  });

  it('updateMember should update an existing member and maintain totalPages', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const memberToUpdate = store.members[0];
    if (memberToUpdate) {
      const updatedName = 'Updated Member';
      const updatedMember: Member = { ...memberToUpdate, fullName: updatedName }; // Changed from name
      await store.updateMember(updatedMember);
      const foundMember = store.getMemberById(memberToUpdate.id);
      expect(foundMember?.fullName).toBe(updatedName);
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(Math.ceil(store.members.length / store.itemsPerPage));
    } else {
      expect.fail('No member to update.');
    }
  });

  it('updateMember should handle empty name correctly', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const memberToUpdate = store.members[0];
    if (memberToUpdate) {
      const updatedMember: Member = { ...memberToUpdate, fullName: '' }; // Changed from name
      await store.updateMember(updatedMember);

      expect(store.members[0].fullName).toBe(memberToUpdate.fullName);
      expect(store.error).toBe('Failed to update member.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('deleteMember should remove a member and update totalPages', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const initialCount = store.members.length;
    const memberToDeleteId = store.members[0]?.id;
    if (memberToDeleteId) {
      await store.deleteMember(memberToDeleteId);
      expect(store.members.length).toBe(initialCount - 1);
      expect(store.getMemberById(memberToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to delete.');
    }
  });

  it('deleteMember should adjust currentPage and totalPages when all members are deleted', async () => {
    const store = useMemberStore();
    // Temporarily set mock service to return fewer members for easier testing of this scenario
    mockMemberService._members = generateMockMembers(1); 
    await store.fetchMembers(); 
    store.setItemsPerPage(1); 
    store.setPage(1);

    await store.deleteMember(mockMemberService.members[0].id);

    expect(store.members.length).toBe(0);
    expect(store.filteredMembers.length).toBe(0);
    expect(store.totalPages).toBe(0);
    expect(store.currentPage).toBe(1);
  });

  it('searchMembers and getFilteredMembers should filter members by fullName or placeOfBirth and reset page', async () => { // Updated test description
    const store = useMemberStore();
    await store.fetchMembers();

    // Search by fullName
    const existingMemberFullName = mockMemberService.members[0].fullName.substring(0, 5); // Changed from name
    store.searchMembers(existingMemberFullName);
    const expectedFilteredCountByName = mockMemberService.members.filter(m => m.fullName.toLowerCase().includes(existingMemberFullName.toLowerCase())).length; // Changed from name
    expect(store.getFilteredMembers.length).toBe(expectedFilteredCountByName);
    expect(store.totalPages).toBe(Math.ceil(expectedFilteredCountByName / store.itemsPerPage));
    expect(store.currentPage).toBe(1);

    // Search by placeOfBirth
    const existingPlaceOfBirth = mockMemberService.members[0].placeOfBirth?.substring(0, 5) || ''; // New search field
    if (existingPlaceOfBirth) {
      store.searchMembers(existingPlaceOfBirth);
      const expectedFilteredCountByPlace = mockMemberService.members.filter(m => m.placeOfBirth?.toLowerCase().includes(existingPlaceOfBirth.toLowerCase())).length;
      expect(store.getFilteredMembers.length).toBe(expectedFilteredCountByPlace);
      expect(store.totalPages).toBe(Math.ceil(expectedFilteredCountByPlace / store.itemsPerPage));
      expect(store.currentPage).toBe(1);
    }

    store.searchMembers('nonexistent');
    expect(store.getFilteredMembers.length).toBe(0);
    expect(store.totalPages).toBe(0);
    expect(store.currentPage).toBe(1);

    store.searchMembers('');
    expect(store.getFilteredMembers.length).toBe(store.members.length);
    expect(store.totalPages).toBe(Math.ceil(store.members.length / store.itemsPerPage));
    expect(store.currentPage).toBe(1);
  });

  it('setCurrentMember should set the current member', () => {
    const store = useMemberStore();
    const mockMember: Member = generateMockMember();
    store.setCurrentMember(mockMember);
    expect(store.currentMember).toEqual(mockMember);

    store.setCurrentMember(null);
    expect(store.currentMember).toBeNull();
  });

  it('paginatedMembers should return members for the current page', async () => {
    const store = useMemberStore();
    await store.fetchMembers();

    expect(store.paginatedMembers.length).toBe(10);
    expect(store.paginatedMembers[0]?.id).toBe(mockMemberService.members[0].id);

    store.setPage(2);
    expect(store.paginatedMembers.length).toBe(10);
    expect(store.paginatedMembers[0]?.id).toBe(mockMemberService.members[10].id);

    store.setPage(3);
    expect(store.currentPage).toBe(2);
  });

  it('setPage should update currentPage', async () => {
    const store = useMemberStore();
    await store.fetchMembers();

    store.setPage(2);
    expect(store.currentPage).toBe(2);

    store.setPage(3);
    expect(store.currentPage).toBe(2);

    store.setPage(0);
    expect(store.currentPage).toBe(2);
  });

  it('setItemsPerPage should update itemsPerPage and totalPages, and adjust currentPage', async () => {
    const store = useMemberStore();
    await store.fetchMembers();

    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(2);

    store.setItemsPerPage(5);
    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(4);
    expect(store.currentPage).toBe(1);

    store.setPage(2);
    store.setItemsPerPage(20);
    expect(store.itemsPerPage).toBe(20);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1);

    store.setItemsPerPage(30);
    expect(store.itemsPerPage).toBe(30);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1);
  });

  it('setItemsPerPage should handle empty members list correctly', async () => {
    const store = useMemberStore();
    mockMemberService._members = [];
    await store.fetchMembers();

    expect(store.members.length).toBe(0);
    expect(store.filteredMembers.length).toBe(0);
    expect(store.totalPages).toBe(0);
    expect(store.currentPage).toBe(1);

    store.setItemsPerPage(10);
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(0);
    expect(store.currentPage).toBe(1);
  });
});
