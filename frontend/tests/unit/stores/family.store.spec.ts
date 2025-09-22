import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useFamilyStore } from '@/stores/family.store'; // Updated import path
import type { Family } from '@/types/family';
import type { IFamilyService } from '@/services';
import { generateMockFamilies, generateMockFamily } from '@/data/mock/family.mock';

// Create a mock service for testing
class MockFamilyServiceForTest implements IFamilyService {
  private _families: Family[] = generateMockFamilies(10); // Use a private variable

  // Getter to return a copy of the families array
  get families(): Family[] {
    return [...this._families]; // Return a shallow copy
  }

  private simulateLatency<T>(data: T): Promise<T> {
    return new Promise((resolve) => setTimeout(() => resolve(data), 0)); // No actual latency needed for tests
  }

  async fetchFamilies(): Promise<Family[]> {
    return this.simulateLatency(this.families); // Return copy
  }
  async getFamilyById(id: string): Promise<Family | undefined> {
    return this.simulateLatency(this.families.find((f) => f.id === id));
  }
  async addFamily(newFamily: Omit<Family, 'id'>): Promise<Family> {
    const familyToAdd = { ...newFamily, id: generateMockFamily().id };
    this._families.push(familyToAdd); // Modify private array
    return this.simulateLatency(familyToAdd);
  }
  async updateFamily(updatedFamily: Family): Promise<Family> {
    const index = this._families.findIndex((f) => f.id === updatedFamily.id);
    if (index !== -1) {
      this._families[index] = updatedFamily;
      return this.simulateLatency(updatedFamily);
    }
    throw new Error('Family not found');
  }
  async deleteFamily(id: string): Promise<void> {
    const initialLength = this._families.length;
    this._families = this._families.filter((f) => f.id !== id);
    if (this._families.length === initialLength) {
      throw new Error('Family not found');
    }
    return this.simulateLatency(undefined);
  }
}

describe('Family Store', () => {
  let mockFamilyService: MockFamilyServiceForTest;

  beforeEach(() => {
    mockFamilyService = new MockFamilyServiceForTest();
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useFamilyStore(); // Get the store instance

    // Directly assign the mock service to the store's services property
    // This bypasses the plugin system for testing, ensuring direct injection.
    store.services = {
      family: mockFamilyService,
    };

    store.$reset(); // Reset store state before each test
  });

  it('should have correct initial state', () => {
    const store = useFamilyStore();
    expect(store.families).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.searchTerm).toBe('');
    expect(store.filteredFamilies).toEqual([]);
    expect(store.currentFamily).toBe(null);
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(0);
  });

  it('fetchFamilies should populate families array, filteredFamilies, and calculate totalPages', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies();
    expect(store.families.length).toBe(10); // Based on mock service initial data
    expect(store.families[0].name).toBe(mockFamilyService.families[0].name);
    expect(store.loading).toBe(false);
    expect(store.filteredFamilies.length).toBe(store.families.length);
    expect(store.totalPages).toBe(2); // 10 families, 5 per page
  });

  it('getFamilyById should return the correct family', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies();
    const family = store.getFamilyById(mockFamilyService.families[0].id);
    expect(family).toBeDefined();
    expect(family?.name).toBe(mockFamilyService.families[0].name);
  });

  it('addFamily should add a new family and update totalPages', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies(); // Ensure some initial data (10 families)
    const initialCount = store.families.length; // 10
    const initialTotalPages = store.totalPages; // 2
    const newFamilyData: Omit<Family, 'id'> = { name: 'The New Family', description: 'A newly added family.' };
    await store.addFamily(newFamilyData);
    expect(store.families.length).toBe(initialCount + 1);
    expect(store.families.some(f => f.name === 'The New Family')).toBe(true);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(3); // 11 families, 5 per page
  });

  it('updateFamily should update an existing family and maintain totalPages', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies();
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

  it('deleteFamily should remove a family and update totalPages', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies(); // 10 families, 2 pages
    const initialCount = store.families.length;
    const familyToDeleteId = store.families[0]?.id;
    if (familyToDeleteId) {
      await store.deleteFamily(familyToDeleteId);
      expect(store.families.length).toBe(initialCount - 1);
      expect(store.getFamilyById(familyToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(2); // 9 families, 5 per page, still 2 pages
    } else {
      expect.fail('No family to delete.');
    }
  });

  it('searchFamilies and getFilteredFamilies should filter families by name or description and reset page', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies(); // 10 families

    // Search for a family name that exists in the mock data
    const existingFamilyName = mockFamilyService.families[0].name.substring(0, 5); // e.g., "Simps"
    store.searchFamilies(existingFamilyName);
    const expectedFilteredCount = mockFamilyService.families.filter(f => f.name.toLowerCase().includes(existingFamilyName.toLowerCase())).length;
    expect(store.getFilteredFamilies.length).toBe(expectedFilteredCount);
    expect(store.totalPages).toBe(Math.ceil(expectedFilteredCount / store.itemsPerPage));
    expect(store.currentPage).toBe(1);

    // No match
    store.searchFamilies('nonexistent');
    expect(store.getFilteredFamilies.length).toBe(0);
    expect(store.totalPages).toBe(0);
    expect(store.currentPage).toBe(1);

    // Clear search
    store.searchFamilies('');
    expect(store.getFilteredFamilies.length).toBe(store.families.length);
    expect(store.totalPages).toBe(Math.ceil(store.families.length / store.itemsPerPage));
    expect(store.currentPage).toBe(1);
  });

  it('setCurrentFamily should set the current family', () => {
    const store = useFamilyStore();
    const mockFamily: Family = generateMockFamily('test-id');
    store.setCurrentFamily(mockFamily);
    expect(store.currentFamily).toEqual(mockFamily);

    store.setCurrentFamily(null);
    expect(store.currentFamily).toBeNull();
  });

  it('paginatedFamilies should return families for the current page', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies(); // 10 families, 5 per page, 2 pages

    // Page 1
    expect(store.paginatedFamilies.length).toBe(5);
    expect(store.paginatedFamilies[0]?.id).toBe(mockFamilyService.families[0].id);
    expect(store.paginatedFamilies[4]?.id).toBe(mockFamilyService.families[4].id);

    // Page 2
    store.setPage(2);
    expect(store.paginatedFamilies.length).toBe(5);
    expect(store.paginatedFamilies[0]?.id).toBe(mockFamilyService.families[5].id);
    expect(store.paginatedFamilies[4]?.id).toBe(mockFamilyService.families[9].id);

    // Invalid page (should not change)
    store.setPage(3);
    expect(store.currentPage).toBe(2);
  });

  it('setPage should update currentPage', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies(); // 10 families, 2 pages

    store.setPage(2);
    expect(store.currentPage).toBe(2);

    // Invalid page (too high)
    store.setPage(3);
    expect(store.currentPage).toBe(2); // Should remain 2

    // Invalid page (too low) - currentPage should not change
    store.setPage(0);
    expect(store.currentPage).toBe(2); // Should remain 2
  });

  it('setItemsPerPage should update itemsPerPage and totalPages, and adjust currentPage', async () => {
    const store = useFamilyStore();
    await store.fetchFamilies(); // 10 families, 5 per page, 2 pages

    expect(store.itemsPerPage).toBe(5);
    expect(store.totalPages).toBe(2);

    // Change to 3 items per page
    store.setItemsPerPage(3); // 10 families, 3 per page -> 4 pages
    expect(store.itemsPerPage).toBe(3);
    expect(store.totalPages).toBe(4);
    expect(store.currentPage).toBe(1); // Should reset to 1 or stay if valid

    // Change to 10 items per page, current page is 1
    store.setPage(2); // Set to page 2
    store.setItemsPerPage(10); // 10 families, 10 per page -> 1 page
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1); // Should adjust to 1 because page 2 is now out of bounds

    // Change to 20 items per page (more than total), current page is 1
    store.setItemsPerPage(20); // 10 families, 20 per page -> 1 page
    expect(store.itemsPerPage).toBe(20);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1); // Should remain 1
  });
});
