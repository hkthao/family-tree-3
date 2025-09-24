import { describe, it, expect, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types/member';
import type { IMemberService, MemberFilter } from '@/services/member/member.service.interface';
import { generateMockMembers, generateMockMember } from '@/data/mock/member.mock';
import { simulateLatency } from '@/utils/mockUtils'; // Import simulateLatency
import { createServices } from '@/services/service.factory';
import type { Paginated } from '@/types/pagination';

// Create a mock service for testing
class MockMemberServiceForTest implements IMemberService {
  private _members: Member[] = generateMockMembers(20); // Use a private variable
  public shouldThrowError: boolean = false;

  // Getter to return a copy of the members array
  get members(): Member[] {
    return [...this._members]; // Return a shallow copy
  }

  async fetch(): Promise<Member[]> { // Renamed from fetchItems
    if (this.shouldThrowError) {
      throw new Error('Không thể tải danh sách thành viên.');
    }
    return simulateLatency(this.members);
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const filteredItems = this.members.filter(member => member.familyId === familyId);
    return simulateLatency(filteredItems);
  }

  async getById(id: string): Promise<Member | undefined> { // Renamed from getItemById
    const member = this.members.find((m) => m.id === id);
    return simulateLatency(member);
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Member> { // Renamed from addItem
    const memberToAdd: Member = {
      ...newItem,
      id: generateMockMember().id,
      dateOfBirth: newItem.dateOfBirth ? new Date(newItem.dateOfBirth) : undefined,
      dateOfDeath: newItem.dateOfDeath ? new Date(newItem.dateOfDeath) : undefined,
    };
    this._members.push(memberToAdd);
    return simulateLatency(memberToAdd);
  }

  async update(updatedItem: Member): Promise<Member> { // Renamed from updateItem
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

  async searchMembers(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Paginated<Member>> {
    let filteredItems = this._members;

    if (filters.fullName) {
      const lowerCaseFullName = filters.fullName.toLowerCase();
      filteredItems = filteredItems.filter(m =>
        m.lastName.toLowerCase().includes(lowerCaseFullName) ||
        m.firstName.toLowerCase().includes(lowerCaseFullName) ||
        `${m.lastName} ${m.firstName}`.toLowerCase().includes(lowerCaseFullName)
      );
    }
    if (filters.dateOfBirth) {
      filteredItems = filteredItems.filter(m => m.dateOfBirth?.toISOString().split('T')[0] === filters.dateOfBirth?.toISOString().split('T')[0]);
    }
    if (filters.dateOfDeath) {
      filteredItems = filteredItems.filter(m => m.dateOfDeath?.toISOString().split('T')[0] === filters.dateOfDeath?.toISOString().split('T')[0]);
    }
    if (filters.gender) {
      filteredItems = filteredItems.filter(m => m.gender === filters.gender);
    }
    if (filters.placeOfBirth) {
      const lowerCasePlaceOfBirth = filters.placeOfBirth.toLowerCase();
      filteredItems = filteredItems.filter(m => m.placeOfBirth?.toLowerCase().includes(lowerCasePlaceOfBirth));
    }
    if (filters.placeOfDeath) {
      const lowerCasePlaceOfDeath = filters.placeOfDeath.toLowerCase();
      filteredItems = filteredItems.filter(m => m.placeOfDeath?.toLowerCase().includes(lowerCasePlaceOfDeath));
    }
    if (filters.occupation) {
      const lowerCaseOccupation = filters.occupation.toLowerCase();
      filteredItems = filteredItems.filter(m => m.occupation?.toLowerCase().includes(lowerCaseOccupation));
    }
    if (filters.familyId) {
      filteredItems = filteredItems.filter(m => m.familyId === filters.familyId);
    }

    const totalItems = filteredItems.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filteredItems.slice(start, end);

    return simulateLatency({
      items,
      totalItems,
      totalPages,
    });
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
    await store._loadItems(); // Ensure store is populated before tests run
  });

  it('should have correct initial state after loading members', () => {
    const store = useMemberStore();
    // After beforeEach, store should be populated
    expect(store.items.length).toBe(store.itemsPerPage); // All members fetched
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
    expect(store.totalItems).toBe(20); // Total items from mock service
    expect(store.totalPages).toBe(2); // 20 members, 10 per page
  });



  it('getItemById should return the correct member', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const member = store.getItemById(mockMemberService.members[0].id);
    expect(member).toBeDefined();
    expect(member?.lastName).toBe(mockMemberService.members[0].lastName);
  });

  it('addItem should add a new member and update members array', async () => {
    const store = useMemberStore();
    await store._loadItems(); // This will now call _loadItems
    const initialTotalItems = store.totalItems; // Use totalItems
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'New',
      firstName: 'Member',
      fullName: 'New Member', // Add fullName property
      familyId: mockMemberService.members[0].familyId,
      dateOfBirth: new Date('2000-01-01'),
    };
    await store.addItem(newMemberData);
    expect(store.totalItems).toBe(initialTotalItems + 1); // Check totalItems
    expect(store.loading).toBe(false);
  });

  it('addItem should set error for empty full name', async () => {
    const store = useMemberStore();
    const newMemberData: Omit<Member, 'id'> = {
      lastName: '',
      firstName: 'Member',
      fullName: 'Member', // Add fullName property
      familyId: mockMemberService.members[0].familyId,
    };
    await store.addItem(newMemberData); // Call the action
    expect(store.error).toBe('Họ và tên không được để trống.');
    expect(store.loading).toBe(false);
  });

  it('addItem should set error for dateOfBirth after dateOfDeath', async () => {
    const store = useMemberStore();
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'Test',
      firstName: 'Member',
      fullName: 'Test Member', // Add fullName property
      familyId: mockMemberService.members[0].familyId,
      dateOfBirth: new Date('2000-01-01'),
      dateOfDeath: new Date('1990-01-01'),
    };
    await store.addItem(newMemberData); // Call the action
    expect(store.error).toBe('Ngày sinh không thể sau ngày mất.');
    expect(store.loading).toBe(false);
  });

  it('addItem should set error for placeOfBirth and placeOfDeath being the same', async () => {
    const store = useMemberStore();
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'Test',
      firstName: 'Member',
      fullName: 'Test Member',
      familyId: mockMemberService.members[0].familyId,
      placeOfBirth: 'Same Place',
      placeOfDeath: 'Same Place',
    };
    await store.addItem(newMemberData);
    expect(store.error).toBe('Nơi sinh và nơi mất không thể giống nhau.');
    expect(store.loading).toBe(false);
  });

  it('addItem should set error for occupation length greater than 100', async () => {
    const store = useMemberStore();
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'Test',
      firstName: 'Member',
      fullName: 'Test Member',
      familyId: mockMemberService.members[0].familyId,
      occupation: 'a'.repeat(101),
    };
    await store.addItem(newMemberData);
    expect(store.error).toBe('Nghề nghiệp không được vượt quá 100 ký tự.');
    expect(store.loading).toBe(false);
  });

  it('updateItem should update an existing member', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const memberToUpdate = store.items[0];
    if (memberToUpdate) {
      const updatedLastName = 'Updated';
      const updatedMember: Member = { ...memberToUpdate, lastName: updatedLastName };
      await store.updateItem(updatedMember);
      await store._loadItems(); // Force re-fetch after update
      const foundMember = store.getItemById(memberToUpdate.id);
      expect(foundMember?.lastName).toBe(updatedLastName);
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('updateItem should set error for empty full name', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const memberToUpdate = store.items[0];
    if (memberToUpdate) {
      const updatedMember: Member = { ...memberToUpdate, lastName: '' };
      await store.updateItem(updatedMember); // Call the action
      expect(store.error).toBe('Họ và tên không được để trống.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('updateItem should set error for dateOfBirth after dateOfDeath', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const memberToUpdate = store.items[0];
    if (memberToUpdate) {
      const updatedMember: Member = {
        ...memberToUpdate,
        dateOfBirth: new Date('2000-01-01'),
        dateOfDeath: new Date('1990-01-01'),
      };
      await store.updateItem(updatedMember); // Call the action
      expect(store.error).toBe('Ngày sinh không thể sau ngày mất.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('updateItem should set error for placeOfBirth and placeOfDeath being the same', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const memberToUpdate = store.items[0];
    if (memberToUpdate) {
      const updatedMember: Member = {
        ...memberToUpdate,
        placeOfBirth: 'Same Place',
        placeOfDeath: 'Same Place',
      };
      await store.updateItem(updatedMember);
      expect(store.error).toBe('Nơi sinh và nơi mất không thể giống nhau.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('updateItem should set error for occupation length greater than 100', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const memberToUpdate = store.items[0];
    if (memberToUpdate) {
      const updatedMember: Member = {
        ...memberToUpdate,
        occupation: 'a'.repeat(101),
      };
      await store.updateItem(updatedMember);
      expect(store.error).toBe('Nghề nghiệp không được vượt quá 100 ký tự.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to update.');
    }
  });

  it('deleteMember should remove a member', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const initialTotalItems = store.totalItems; // Use totalItems
    const memberToDeleteId = store.items[0]?.id;
    if (memberToDeleteId) {
      await store.deleteItem(memberToDeleteId);
      expect(store.totalItems).toBe(initialTotalItems - 1); // Check totalItems
      expect(store.getItemById(memberToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No member to delete.');
    }
  });

  it('searchItems should filter members by fullName', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const existingMember = mockMemberService.members[0];
    const searchName = existingMember.lastName.substring(0, 3); // Search by part of last name

    await store.searchItems({ fullName: searchName });
    const expectedFilteredCount = mockMemberService.members.filter(m =>
      m.lastName.toLowerCase().includes(searchName.toLowerCase()) ||
      m.firstName.toLowerCase().includes(searchName.toLowerCase()) ||
      `${m.lastName} ${m.firstName}`.toLowerCase().includes(searchName.toLowerCase())
    ).length;

    expect(store.totalItems).toBe(expectedFilteredCount); // Check totalItems
    expect(store.currentPage).toBe(1);
    expect(store.filters.fullName).toBe(searchName);
  });

  it('searchItems should filter members by dateOfBirth', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const existingMember = mockMemberService.members.find(m => m.dateOfBirth !== undefined);
    if (!existingMember || !existingMember.dateOfBirth) {
      expect.fail('No member with dateOfBirth found in mock data.');
    }
    const searchDate = existingMember.dateOfBirth;

    await store.searchItems({ dateOfBirth: searchDate });
    const expectedFilteredCount = mockMemberService.members.filter(m =>
      m.dateOfBirth?.toISOString().split('T')[0] === searchDate.toISOString().split('T')[0]
    ).length;

    expect(store.totalItems).toBe(expectedFilteredCount); // Check totalItems
    expect(store.currentPage).toBe(1);
    expect(store.filters.dateOfBirth?.toISOString().split('T')[0]).toBe(searchDate.toISOString().split('T')[0]);
  });

  it('searchItems should filter members by gender', async () => {
    const store = useMemberStore();
    await store._loadItems();
    const searchGender = 'male';

    await store.searchItems({ gender: searchGender });
    const expectedFilteredCount = mockMemberService.members.filter(m => m.gender === searchGender).length;

    expect(store.totalItems).toBe(expectedFilteredCount); // Check totalItems
    expect(store.currentPage).toBe(1);
    expect(store.filters.gender).toBe(searchGender);
  });

  it('setPage should update currentPage and affect paginatedItems', async () => {
    const store = useMemberStore();
    await store._loadItems(); // 20 members, 10 per page, 2 pages

    store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(store.paginatedItems.length).toBe(10); // Second page of 10 items

    // Invalid page (too high)
    store.setPage(3);
    expect(store.currentPage).toBe(2);

    // Invalid page (too low)
    store.setPage(0);
    expect(store.currentPage).toBe(2);
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and affect paginatedItems', async () => {
    const store = useMemberStore();
    await store._loadItems(); // 20 members, 10 per page, 2 pages

    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(2);

    // Change to 5 items per page
    await store.setItemsPerPage(5); // 20 members, 5 per page -> 4 pages
    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(4);
    expect(store.currentPage).toBe(1);

    // Change to 20 items per page, current page is 1
    await store.setItemsPerPage(20); // 20 members, 20 per page -> 1 page
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(20);
    expect(store.totalPages).toBe(1);
  });

  it('setCurrentItem should set the current item', () => {
    const store = useMemberStore();
    const mockMember: Member = generateMockMember('test-family-id');
    store.setCurrentItem(mockMember);
    expect(store.currentItem).toEqual(mockMember);

    store.setCurrentItem(null);
    expect(store.currentItem).toBeNull();
  });

  it('fetchItems should set error and loading to false on fetch failure', async () => {
    const store = useMemberStore();
    mockMemberService.shouldThrowError = true;
    await store.fetchItems();
    expect(store.error).toBe('Không thể tải danh sách thành viên.');
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]);
  });
});