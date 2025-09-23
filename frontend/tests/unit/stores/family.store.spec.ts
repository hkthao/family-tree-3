import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach } from 'vitest';
import type { Family } from '@/types/family';
import type { IFamilyService } from '@/services/family/family.service.interface'; // Removed PaginatedFamilies
import {
  generateMockFamilies,
  generateMockFamily,
} from '@/data/mock/family.mock';
import type { Paginated } from '@/types/pagination'; // Import generic Paginated interface
import { useFamilyStore } from '@/stores/family.store';
import { simulateLatency } from '@/utils/mockUtils'; // Import simulateLatency
import { createServices } from '@/services/service.factory';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

const TOTAL_ITEMS = 20;
const ITEMS_PER_PAGE = DEFAULT_ITEMS_PER_PAGE;

// Create a mock service for testing
class MockFamilyServiceForTest implements IFamilyService {
  private _items: Family[] = generateMockFamilies(TOTAL_ITEMS); // Use a private variable

  // Getter to return a copy of the items array
  get items(): Family[] {
    return [...this._items]; // Return a shallow copy
  }

  async fetch(): Promise<Family[]> {
    // Renamed from fetchFamilies
    return simulateLatency(this.items);
  }
  async getById(id: string): Promise<Family | undefined> {
    // Renamed from getFamilyById
    return simulateLatency(this.items.find((f) => f.id === id));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Family> {
    // Renamed from addFamily
    const familyToAdd = { ...newItem, id: generateMockFamily().id };
    this._items.push(familyToAdd);
    return simulateLatency(familyToAdd);
  }
  async update(updatedItem: Family): Promise<Family> {
    // Renamed from updateFamily
    const index = this._items.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this._items[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Family not found');
  }
  async delete(id: string): Promise<void> {
    // Renamed from deleteFamily
    const initialLength = this._items.length;
    this._items = this._items.filter((f) => f.id !== id);
    if (this._items.length === initialLength) {
      throw new Error('Family not found');
    }
    return simulateLatency(undefined);
  }

  async searchItems(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number,
  ): Promise<Paginated<Family>> {
    // Use generic Paginated interface
    let filtered = this._items;

    // Filter by searchQuery
    if (searchQuery) {
      const lowerCaseSearchQuery = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (family.description &&
            family.description.toLowerCase().includes(lowerCaseSearchQuery)),
      );
    }

    // Filter by visibility (assuming Family has a 'visibility' property)
    if (visibility !== 'all') {
      filtered = filtered.filter((family) => family.visibility === visibility);
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

describe('Family Store', () => {
  let mockFamilyService: MockFamilyServiceForTest;

  beforeEach(async () => {
    mockFamilyService = new MockFamilyServiceForTest();
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useFamilyStore(); // Get the store instance

    store.$reset(); // Reset store state before each test
    // Pass mockMemberService to createServices

    store.services = createServices('test', {
      family: mockFamilyService,
    });

    await store._loadItems(); // Ensure store is populated before tests run
  });

  it('should have correct state after initial load', () => {
    const store = useFamilyStore();
    // After beforeEach, store should be populated
    expect(store.items.length).toBe(DEFAULT_ITEMS_PER_PAGE); // ITEMS_PER_PAGE items per page
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.searchTerm).toBe('');
    expect(store.visibilityFilter).toBe('all');
    expect(store.totalItems).toBe(TOTAL_ITEMS); // Based on mock service initial data
    expect(store.currentItem).toBe(null);
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(2); // TOTAL_ITEMS items, ITEMS_PER_PAGE per page
  });

  it('_loadItems should populate items array, totalItems, and totalPages', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Call the renamed internal method
    expect(store.items.length).toBe(ITEMS_PER_PAGE); // ITEMS_PER_PAGE items per page
    expect(store.totalItems).toBe(TOTAL_ITEMS); // Based on mock service initial data
    expect(store.items[0].name).toBe(mockFamilyService.items[0].name);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(2); // TOTAL_ITEMS items, ITEMS_PER_PAGE per page
  });

  it('getFamilyById should return the correct family', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Updated call
    const family = store.getItemById(mockFamilyService.items[0].id);
    expect(family).toBeDefined();
    expect(family?.name).toBe(mockFamilyService.items[0].name);
  });

  it('addFamily should add a new family and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
    const initialTotalItems = store.totalItems; // TOTAL_ITEMS
    const newFamilyData: Omit<Family, 'id'> = {
      name: 'The New Family',
      description: 'A newly added family.',
    };
    await store.addItem(newFamilyData);
    expect(store.totalItems).toBe(initialTotalItems + 1); // 11
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(3); // 11 items, ITEMS_PER_PAGE per page

    // Now, search for the newly added family to confirm its presence
    await store.searchItems('The New Family', 'all');
    expect(store.totalItems).toBe(1);
    expect(store.items[0].name).toBe('The New Family');
  });

  it('updateFamily should update an existing family and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems();
    const familyToUpdate = store.items[0];
    if (familyToUpdate) {
      const updatedName = 'The Updated Family';
      const updatedFamily: Family = { ...familyToUpdate, name: updatedName };
      await store.updateItem(updatedFamily);
      const foundFamily = store.getItemById(familyToUpdate.id);
      expect(foundFamily?.name).toBe(updatedName);
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(2); // Should remain 2
    } else {
      expect.fail('No family to update.');
    }
  });

  it('deleteFamily should remove a family and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // TOTAL_ITEMS items, 2 pages
    const initialTotalItems = store.totalItems;
    const familyToDeleteId = store.items[0]?.id;
    if (familyToDeleteId) {
      await store.deleteItem(familyToDeleteId);
      expect(store.totalItems).toBe(initialTotalItems - 1); // 9
      expect(store.getItemById(familyToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
      expect(store.totalPages).toBe(2); // 9 items, ITEMS_PER_PAGE per page, still 2 pages
    } else {
      expect.fail('No family to delete.');
    }
  });

  it('searchItems should filter items by search term and reset page', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // TOTAL_ITEMS items

    const existingFamilyName = mockFamilyService.items[0].name.substring(
      0,
      ITEMS_PER_PAGE,
    );
    await store.searchItems(existingFamilyName, 'all');
    const expectedFilteredCount = mockFamilyService.items.filter((f) =>
      f.name.toLowerCase().includes(existingFamilyName.toLowerCase()),
    ).length;
    expect(store.totalItems).toBe(expectedFilteredCount);
    expect(store.items.length).toBe(
      Math.min(store.itemsPerPage, expectedFilteredCount),
    );
    expect(store.currentPage).toBe(1);
    expect(store.searchTerm).toBe(existingFamilyName);
    expect(store.visibilityFilter).toBe('all');
  });

  it('searchItems should filter items by visibility', async () => {
    const store = useFamilyStore();
    await store._loadItems();

    await store.searchItems('', 'public');
    const publicFamilies = mockFamilyService.items.filter(
      (f) => f.visibility === 'public',
    );
    expect(store.totalItems).toBe(publicFamilies.length);
    expect(store.items.every((f) => f.visibility === 'public')).toBe(true);
    expect(store.currentPage).toBe(1);
    expect(store.visibilityFilter).toBe('public');
  });

  it('searchItems should filter items by search term and visibility', async () => {
    const store = useFamilyStore();
    await store._loadItems();

    const existingFamilyName = mockFamilyService.items[0].name.substring(
      0,
      ITEMS_PER_PAGE,
    );
    await store.searchItems(existingFamilyName, 'private');
    const privateFamiliesMatchingSearch = mockFamilyService.items.filter(
      (f) =>
        f.visibility === 'private' &&
        f.name.toLowerCase().includes(existingFamilyName.toLowerCase()),
    );
    expect(store.totalItems).toBe(privateFamiliesMatchingSearch.length);
    expect(
      store.items.every(
        (f) =>
          f.visibility === 'private' &&
          f.name.toLowerCase().includes(existingFamilyName.toLowerCase()),
      ),
    ).toBe(true);
    expect(store.currentPage).toBe(1);
    expect(store.searchTerm).toBe(existingFamilyName);
    expect(store.visibilityFilter).toBe('private');
  });

  it('setCurrentItem should set the current family', () => {
    const store = useFamilyStore();
    const mockFamily: Family = generateMockFamily('test-id');
    store.setCurrentItem(mockFamily);
    expect(store.currentItem).toEqual(mockFamily);

    store.setCurrentItem(null);
    expect(store.currentItem).toBeNull();
  });

  it('setPage should update currentPage and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // TOTAL_ITEMS items, ITEMS_PER_PAGE per page, 2 pages

    await store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(store.items.length).toBe(ITEMS_PER_PAGE); // Second page of ITEMS_PER_PAGE items
    expect(store.items[0]?.id).toBe(mockFamilyService.items[ITEMS_PER_PAGE].id); // First item of second page

    // Invalid page (too high)
    await store.setPage(3);
    expect(store.currentPage).toBe(2); // Should remain 2

    // Invalid page (too low) - currentPage should not change
    await store.setPage(0);
    expect(store.currentPage).toBe(2); // Should remain 2
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // TOTAL_ITEMS items, ITEMS_PER_PAGE per page, 2 pages

    expect(store.itemsPerPage).toBe(ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(2);

    // Change to 3 items per page
    await store.setItemsPerPage(3); // 20 items, 3 per page -> 7 pages
    expect(store.itemsPerPage).toBe(3);
    expect(store.totalPages).toBe(7);
    expect(store.currentPage).toBe(1); // Should reset to 1

    // Change back to default items per page
    await store.setItemsPerPage(ITEMS_PER_PAGE); // 20 items, 10 per page -> 2 pages
    expect(store.itemsPerPage).toBe(ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(2);
    expect(store.currentPage).toBe(1); // Should remain 1
  });
});
