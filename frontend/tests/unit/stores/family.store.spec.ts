import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach } from 'vitest';
import type { Family } from '@/types/family';
import type { IFamilyService } from '@/services/family/family.service.interface'; // Removed PaginatedFamilies
import { generateMockFamilies, generateMockFamily } from '@/data/mock/family.mock';
import type { Paginated } from '@/types/pagination'; // Import generic Paginated interface
import { useFamilyStore } from '@/stores/family.store';
import { simulateLatency } from '@/utils/mockUtils'; // Import simulateLatency
import { createServices } from '@/services/service.factory';
import type { IMemberService } from '@/services/member/member.service.interface';
import type { Member } from '@/types/member';

// Create a mock service for testing
class MockFamilyServiceForTest implements IFamilyService {
  private _families: Family[] = generateMockFamilies(10); // Use a private variable

  // Getter to return a copy of the families array
  get families(): Family[] {
    return [...this._families]; // Return a shallow copy
  }

  async fetch(): Promise<Family[]> { // Renamed from fetchFamilies
    return simulateLatency(this.families);
  }
  async getById(id: string): Promise<Family | undefined> { // Renamed from getFamilyById
    return simulateLatency(this.families.find((f) => f.id === id));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Family> { // Renamed from addFamily
    const familyToAdd = { ...newItem, id: generateMockFamily().id };
    this._families.push(familyToAdd);
    return simulateLatency(familyToAdd);
  }
  async update(updatedItem: Family): Promise<Family> { // Renamed from updateFamily
    const index = this._families.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this._families[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Family not found');
  }
  async delete(id: string): Promise<void> { // Renamed from deleteFamily
    const initialLength = this._families.length;
    this._families = this._families.filter((f) => f.id !== id);
    if (this._families.length === initialLength) {
      throw new Error('Family not found');
    }
    return simulateLatency(undefined);
  }

  async searchFamilies(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Family>> { // Use generic Paginated interface
    let filtered = this._families;

    // Filter by searchQuery
    if (searchQuery) {
      const lowerCaseSearchQuery = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (family.description && family.description.toLowerCase().includes(lowerCaseSearchQuery))
      );
    }

    // Filter by visibility (assuming Family has a 'visibility' property)
    if (visibility !== 'all') {
      // This assumes Family has a 'visibility' property. If not, this will need adjustment.
      filtered = filtered.filter((family) => (family as any).visibility === visibility);
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return simulateLatency({
      items,
      totalItems,
      totalPages,
    });
  }
}

class MockMemberServiceForTest implements IMemberService {
  async fetch(): Promise<Member[]> {
    return [];
  }
  async getById(id: string): Promise<Member | undefined> {
    return undefined;
  }
  async add(newItem: Omit<Member, 'id'>): Promise<Member> {
    throw new Error('Method not implemented.');
  }
  async update(updatedItem: Member): Promise<Member> {
    throw new Error('Method not implemented.');
  }
  async delete(id: string): Promise<void> {
    throw new Error('Method not implemented.');
  }
  async searchMembers(
    searchQuery: string,
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Member>> {
    return { items: [], totalItems: 0, totalPages: 0 };
  }
}

describe('Family Store', () => {
  let mockFamilyService: MockFamilyServiceForTest;
  let mockMemberService: MockMemberServiceForTest; // Declare mockMemberService

  beforeEach(async () => {
    mockFamilyService = new MockFamilyServiceForTest();
    mockMemberService = new MockMemberServiceForTest(); // Instantiate mockMemberService
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useFamilyStore(); // Get the store instance

    store.$reset(); // Reset store state before each test
    // Pass mockMemberService to createServices
    store.services = createServices('test', mockFamilyService, mockMemberService);

    await store._loadFamilies(); // Ensure store is populated before tests run
  });

  it('should have correct state after initial load', () => {
    const store = useFamilyStore();
    // After beforeEach, store should be populated
    expect(store.families.length).toBe(5); // 5 items per page
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.searchTerm).toBe('');
    expect(store.visibilityFilter).toBe('all');
    expect(store.totalItems).toBe(10); // Based on mock service initial data
    expect(store.currentFamily).toBe(null);
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(2); // 10 families, 5 per page
  });

  it('_loadFamilies should populate families array, totalItems, and totalPages', async () => {
    const store = useFamilyStore();
    await store._loadFamilies(); // Call the renamed internal method
    expect(store.families.length).toBe(5); // 5 items per page
    expect(store.totalItems).toBe(10); // Based on mock service initial data
    expect(store.families[0].name).toBe(mockFamilyService.families[0].name);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(2); // 10 families, 5 per page
  });

  it('getFamilyById should return the correct family', async () => {
    const store = useFamilyStore();
    await store._loadFamilies(); // Updated call
    const family = store.getFamilyById(mockFamilyService.families[0].id);
    expect(family).toBeDefined();
    expect(family?.name).toBe(mockFamilyService.families[0].name);
  });

  it('addFamily should add a new family and re-load families', async () => {
    const store = useFamilyStore();
    await store._loadFamilies(); // Initial fetch
    const initialTotalItems = store.totalItems; // 10
    const newFamilyData: Omit<Family, 'id'> = { name: 'The New Family', description: 'A newly added family.' };
    await store.addFamily(newFamilyData);
    expect(store.totalItems).toBe(initialTotalItems + 1); // 11
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(3); // 11 families, 5 per page

    // Now, search for the newly added family to confirm its presence
    await store.searchFamilies('The New Family', 'all');
    expect(store.totalItems).toBe(1);
    expect(store.families[0].name).toBe('The New Family');
  });

  it('updateFamily should update an existing family and re-load families', async () => {
    const store = useFamilyStore();
    await store._loadFamilies();
    const familyToUpdate = store.families[0];
    if (familyToUpdate) {
      const updatedName = 'The Updated Family';
      const updatedFamily: Family = { ...familyToUpdate, name: updatedName };
      await store.updateFamily(updatedFamily);
      const foundFamily = store.getFamilyById(familyToUpdate.id);
      expect(foundFamily?.name).toBe(updatedName);
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(2); // Should remain 2
    } else {
      expect.fail('No family to update.');
    }
  });

  it('deleteFamily should remove a family and re-load families', async () => {
    const store = useFamilyStore();
    await store._loadFamilies(); // 10 families, 2 pages
    const initialTotalItems = store.totalItems;
    const familyToDeleteId = store.families[0]?.id;
    if (familyToDeleteId) {
      await store.deleteFamily(familyToDeleteId);
      expect(store.totalItems).toBe(initialTotalItems - 1); // 9
      expect(store.getFamilyById(familyToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(2); // 9 families, 5 per page, still 2 pages
    } else {
      expect.fail('No family to delete.');
    }
  });

  it('searchFamilies should filter families by search term and reset page', async () => {
    const store = useFamilyStore();
    await store._loadFamilies(); // 10 families

    const existingFamilyName = mockFamilyService.families[0].name.substring(0, 5);
    await store.searchFamilies(existingFamilyName, 'all');
    const expectedFilteredCount = mockFamilyService.families.filter(f => f.name.toLowerCase().includes(existingFamilyName.toLowerCase())).length;
    expect(store.totalItems).toBe(expectedFilteredCount);
    expect(store.families.length).toBe(Math.min(store.itemsPerPage, expectedFilteredCount));
    expect(store.currentPage).toBe(1);
    expect(store.searchTerm).toBe(existingFamilyName);
    expect(store.visibilityFilter).toBe('all');
  });

  it('searchFamilies should filter families by visibility', async () => {
    const store = useFamilyStore();
    await store._loadFamilies();

    await store.searchFamilies('', 'public');
    const publicFamilies = mockFamilyService.families.filter(f => f.visibility === 'public');
    expect(store.totalItems).toBe(publicFamilies.length);
    expect(store.families.every(f => f.visibility === 'public')).toBe(true);
    expect(store.currentPage).toBe(1);
    expect(store.visibilityFilter).toBe('public');
  });

  it('searchFamilies should filter families by search term and visibility', async () => {
    const store = useFamilyStore();
    await store._loadFamilies();

    const existingFamilyName = mockFamilyService.families[0].name.substring(0, 5);
    await store.searchFamilies(existingFamilyName, 'private');
    const privateFamiliesMatchingSearch = mockFamilyService.families.filter(
      f => f.visibility === 'private' && f.name.toLowerCase().includes(existingFamilyName.toLowerCase())
    );
    expect(store.totalItems).toBe(privateFamiliesMatchingSearch.length);
    expect(store.families.every(f => f.visibility === 'private' && f.name.toLowerCase().includes(existingFamilyName.toLowerCase()))).toBe(true);
    expect(store.currentPage).toBe(1);
    expect(store.searchTerm).toBe(existingFamilyName);
    expect(store.visibilityFilter).toBe('private');
  });

  it('setCurrentFamily should set the current family', () => {
    const store = useFamilyStore();
    const mockFamily: Family = generateMockFamily('test-id');
    store.setCurrentFamily(mockFamily);
    expect(store.currentFamily).toEqual(mockFamily);

    store.setCurrentFamily(null);
    expect(store.currentFamily).toBeNull();
  });

  it('setPage should update currentPage and re-load families', async () => {
    const store = useFamilyStore();
    await store._loadFamilies(); // 10 families, 5 per page, 2 pages

    await store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(store.families.length).toBe(5); // Second page of 5 items
    expect(store.families[0]?.id).toBe(mockFamilyService.families[5].id); // First item of second page

    // Invalid page (too high)
    await store.setPage(3);
    expect(store.currentPage).toBe(2); // Should remain 2

    // Invalid page (too low) - currentPage should not change
    await store.setPage(0);
    expect(store.currentPage).toBe(2); // Should remain 2
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and re-load families', async () => {
    const store = useFamilyStore();
    await store._loadFamilies(); // 10 families, 5 per page, 2 pages

    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(2);

    // Change to 3 items per page
    await store.setItemsPerPage(3); // 10 families, 3 per page -> 4 pages
    expect(store.itemsPerPage).toBe(3);
    expect(store.totalPages).toBe(4);
    expect(store.currentPage).toBe(1); // Should reset to 1

    // Change to 10 items per page, current page is 1
    await store.setItemsPerPage(10); // 10 families, 10 per page -> 1 page
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1); // Should remain 1

    // Change to 20 items per page (more than total), current page is 1
    await store.setItemsPerPage(20); // 10 families, 20 per page -> 1 page
    expect(store.itemsPerPage).toBe(20);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1); // Should remain 1
  });
});
