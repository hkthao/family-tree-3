import { describe, it, expect, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types/member';
import type { IMemberService, MemberFilter } from '@/services/member/member.service.interface';
import { generateMockMembers, generateMockMember } from '@/data/mock/member.mock';
import { simulateLatency } from '@/utils/mockUtils'; // Import simulateLatency
import { createServices } from '@/services/service.factory';

// Create a mock service for testing
class MockMemberServiceForTest implements IMemberService {
  private _members: Member[] = generateMockMembers(20); // Use a private variable

  // Getter to return a copy of the members array
  get members(): Member[] {
    return [...this._members]; // Return a shallow copy
  }

  async fetch(): Promise<Member[]> { // Renamed from fetchMembers
    return simulateLatency(this.members);
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const filteredMembers = this.members.filter(member => member.familyId === familyId);
    return simulateLatency(filteredMembers);
  }

  async getById(id: string): Promise<Member | undefined> { // Renamed from getMemberById
    const member = this.members.find((m) => m.id === id);
    return simulateLatency(member);
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Member> { // Renamed from addMember
    const memberToAdd: Member = {
      ...newItem,
      id: generateMockMember().id,
      dateOfBirth: newItem.dateOfBirth ? new Date(newItem.dateOfBirth) : undefined,
      dateOfDeath: newItem.dateOfDeath ? new Date(newItem.dateOfDeath) : undefined,
    };
    this._members.push(memberToAdd);
    return simulateLatency(memberToAdd);
  }

  async update(updatedItem: Member): Promise<Member> { // Renamed from updateMember
    const index = this._members.findIndex((m) => m.id === updatedItem.id);
    if (index !== -1) {
      const memberToUpdate: Member = {
        ...updatedItem,
        dateOfBirth: updatedItem.dateOfBirth ? new Date(updatedItem.dateOfBirth) : undefined,
        dateOfDeath: updatedItem.dateOfDeath ? new Date(updatedItem.dateOfDeath) : undefined,
      };
      this._members[index] = memberToUpdate;
      return simulateLatency(memberToUpdate);
    }
    throw new Error('Member not found');
  }

  async delete(id: string): Promise<void> { // Renamed from deleteMember
    const initialLength = this._members.length;
    this._members = this._members.filter((m) => m.id !== id);
    if (this.members.length === initialLength) {
      throw new Error('Member not found');
    }
    return simulateLatency(undefined);
  }

  async searchMembers(filters: MemberFilter): Promise<Member[]> {
    let filteredMembers = this._members;

    if (filters.fullName) {
      const lowerCaseFullName = filters.fullName.toLowerCase();
      filteredMembers = filteredMembers.filter(m =>
        m.lastName.toLowerCase().includes(lowerCaseFullName) ||
        m.firstName.toLowerCase().includes(lowerCaseFullName) ||
        `${m.lastName} ${m.firstName}`.toLowerCase().includes(lowerCaseFullName)
      );
    }
    if (filters.dateOfBirth) {
      filteredMembers = filteredMembers.filter(m => m.dateOfBirth?.toISOString().split('T')[0] === filters.dateOfBirth?.toISOString().split('T')[0]);
    }
    if (filters.dateOfDeath) {
      filteredMembers = filteredMembers.filter(m => m.dateOfDeath?.toISOString().split('T')[0] === filters.dateOfDeath?.toISOString().split('T')[0]);
    }
    if (filters.gender) {
      filteredMembers = filteredMembers.filter(m => m.gender === filters.gender);
    }
    if (filters.placeOfBirth) {
      const lowerCasePlaceOfBirth = filters.placeOfBirth.toLowerCase();
      filteredMembers = filteredMembers.filter(m => m.placeOfBirth?.toLowerCase().includes(lowerCasePlaceOfBirth));
    }
    if (filters.placeOfDeath) {
      const lowerCasePlaceOfDeath = filters.placeOfDeath.toLowerCase();
      filteredMembers = filteredMembers.filter(m => m.placeOfDeath?.toLowerCase().includes(lowerCasePlaceOfDeath));
    }
    if (filters.occupation) {
      const lowerCaseOccupation = filters.occupation.toLowerCase();
      filteredMembers = filteredMembers.filter(m => m.occupation?.toLowerCase().includes(lowerCaseOccupation));
    }
    if (filters.familyId) {
      filteredMembers = filteredMembers.filter(m => m.familyId === filters.familyId);
    }

    return simulateLatency(filteredMembers);
  }
}

describe('Member Store', () => {
  let mockMemberService: MockMemberServiceForTest;

  beforeEach(async () => {
    mockMemberService = new MockMemberServiceForTest();
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useMemberStore();
    store.$reset(); // Reset store state before each test
    store.services = createServices('test', { member: mockMemberService });
    await store.fetchMembers(); // Ensure store is populated before tests run
  });

  it('should have correct initial state after loading members', () => {
    const store = useMemberStore();
    // After beforeEach, store should be populated
    expect(store.members.length).toBe(20); // All members fetched
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.filters).toEqual({
      fullName: '',
      dateOfBirth: null,
      dateOfDeath: null,
      gender: undefined,
      placeOfBirth: '',
      placeOfDeath: '',
      occupation: '',
      familyId: undefined,
    });
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(2); // 20 members, 10 per page
  });

  it('fetchMembers should populate members array and reset currentPage', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    expect(store.members.length).toBe(20); // Based on mock service initial data
    expect(store.loading).toBe(false);
    expect(store.currentPage).toBe(1);
  });

  it('fetchMembersByFamilyId should populate members array with filtered members', async () => {
    const store = useMemberStore();
    const familyId = mockMemberService.members[0].familyId;
    await store.fetchMembers(familyId);
    const expectedMembers = mockMemberService.members.filter(m => m.familyId === familyId);
    expect(store.members.length).toBe(expectedMembers.length);
    expect(store.members.every(m => m.familyId === familyId)).toBe(true);
    expect(store.loading).toBe(false);
    expect(store.currentPage).toBe(1);
  });

  it('getMemberById should return the correct member', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const member = store.getMemberById(mockMemberService.members[0].id);
    expect(member).toBeDefined();
    expect(member?.lastName).toBe(mockMemberService.members[0].lastName);
  });

  it('addMember should add a new member and update members array', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const initialCount = store.members.length;
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'New',
      firstName: 'Member',
      fullName: 'New Member', // Add fullName property
      familyId: mockMemberService.members[0].familyId,
      dateOfBirth: new Date('2000-01-01'),
    };
    await store.addMember(newMemberData);
    expect(store.members.length).toBe(initialCount + 1);
    expect(store.members.some(m => m.lastName === 'New' && m.firstName === 'Member')).toBe(true);
    expect(store.loading).toBe(false);
  });

  it('addMember should set error for empty full name', async () => {
    const store = useMemberStore();
    const newMemberData: Omit<Member, 'id'> = {
      lastName: '',
      firstName: 'Member',
      fullName: 'Member', // Add fullName property
      familyId: mockMemberService.members[0].familyId,
    };
    await store.addMember(newMemberData); // Call the action
    expect(store.error).toBe('Họ và tên không được để trống.');
    expect(store.loading).toBe(false);
  });

  it('addMember should set error for dateOfBirth after dateOfDeath', async () => {
    const store = useMemberStore();
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'Test',
      firstName: 'Member',
      fullName: 'Test Member', // Add fullName property
      familyId: mockMemberService.members[0].familyId,
      dateOfBirth: new Date('2000-01-01'),
      dateOfDeath: new Date('1990-01-01'),
    };
    await store.addMember(newMemberData); // Call the action
    expect(store.error).toBe('Ngày sinh không thể sau ngày mất.');
    expect(store.loading).toBe(false);
  });

  it('updateMember should update an existing member', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const memberToUpdate = store.members[0];
    if (memberToUpdate) {
      const updatedLastName = 'Updated';
      const updatedMember: Member = { ...memberToUpdate, lastName: updatedLastName };
      await store.updateMember(updatedMember);
      await store.fetchMembers(); // Force re-fetch after update
      const foundMember = store.getMemberById(memberToUpdate.id);
      expect(foundMember?.lastName).toBe(updatedLastName);
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('updateMember should set error for empty full name', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const memberToUpdate = store.members[0];
    if (memberToUpdate) {
      const updatedMember: Member = { ...memberToUpdate, lastName: '' };
      await store.updateMember(updatedMember); // Call the action
      expect(store.error).toBe('Họ và tên không được để trống.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('updateMember should set error for dateOfBirth after dateOfDeath', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const memberToUpdate = store.members[0];
    if (memberToUpdate) {
      const updatedMember: Member = {
        ...memberToUpdate,
        dateOfBirth: new Date('2000-01-01'),
        dateOfDeath: new Date('1990-01-01'),
      };
      await store.updateMember(updatedMember); // Call the action
      expect(store.error).toBe('Ngày sinh không thể sau ngày mất.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('deleteMember should remove a member', async () => {
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

  it('searchMembers should filter members by fullName', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const existingMember = mockMemberService.members[0];
    const searchName = existingMember.lastName.substring(0, 3); // Search by part of last name

    store.searchMembers({ fullName: searchName });
    const expectedFilteredCount = mockMemberService.members.filter(m =>
      m.lastName.toLowerCase().includes(searchName.toLowerCase()) ||
      m.firstName.toLowerCase().includes(searchName.toLowerCase()) ||
      `${m.lastName} ${m.firstName}`.toLowerCase().includes(searchName.toLowerCase())
    ).length;

    expect(store.filteredMembers.length).toBe(expectedFilteredCount);
    expect(store.currentPage).toBe(1);
    expect(store.filters.fullName).toBe(searchName);
  });

  it('searchMembers should filter members by dateOfBirth', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const existingMember = mockMemberService.members.find(m => m.dateOfBirth !== undefined);
    if (!existingMember || !existingMember.dateOfBirth) {
      expect.fail('No member with dateOfBirth found in mock data.');
    }
    const searchDate = existingMember.dateOfBirth;

    store.searchMembers({ dateOfBirth: searchDate });
    const expectedFilteredCount = mockMemberService.members.filter(m =>
      m.dateOfBirth?.toISOString().split('T')[0] === searchDate.toISOString().split('T')[0]
    ).length;

    expect(store.filteredMembers.length).toBe(expectedFilteredCount);
    expect(store.currentPage).toBe(1);
    expect(store.filters.dateOfBirth?.toISOString().split('T')[0]).toBe(searchDate.toISOString().split('T')[0]);
  });

  it('searchMembers should filter members by gender', async () => {
    const store = useMemberStore();
    await store.fetchMembers();
    const searchGender = 'male';

    store.searchMembers({ gender: searchGender });
    const expectedFilteredCount = mockMemberService.members.filter(m => m.gender === searchGender).length;

    expect(store.filteredMembers.length).toBe(expectedFilteredCount);
    expect(store.currentPage).toBe(1);
    expect(store.filters.gender).toBe(searchGender);
  });

  it('setPage should update currentPage and affect paginatedMembers', async () => {
    const store = useMemberStore();
    await store.fetchMembers(); // 20 members, 10 per page, 2 pages

    store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(store.paginatedMembers.length).toBe(10); // Second page of 10 items
    expect(store.paginatedMembers[0]?.id).toBe(mockMemberService.members[10].id); // First item of second page

    // Invalid page (too high)
    store.setPage(3);
    expect(store.currentPage).toBe(2);

    // Invalid page (too low)
    store.setPage(0);
    expect(store.currentPage).toBe(2);
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and affect paginatedMembers', async () => {
    const store = useMemberStore();
    await store.fetchMembers(); // 20 members, 10 per page, 2 pages

    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(2);

    // Change to 5 items per page
    store.setItemsPerPage(5); // 20 members, 5 per page -> 4 pages
    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(4);
    expect(store.currentPage).toBe(1);

    // Change to 20 items per page, current page is 1
    store.setItemsPerPage(20); // 20 members, 20 per page -> 1 page
    expect(store.itemsPerPage).toBe(20);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1);
  });

  it('setCurrentMember should set the current member', () => {
    const store = useMemberStore();
    const mockMember: Member = generateMockMember('test-family-id');
    store.setCurrentMember(mockMember);
    expect(store.currentMember).toEqual(mockMember);

    store.setCurrentMember(null);
    expect(store.currentMember).toBeNull();
  });
});