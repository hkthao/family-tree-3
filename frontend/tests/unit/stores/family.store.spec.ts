import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach } from 'vitest';
import type { Family, FamilySearchFilter } from '@/types/family';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { generateMockFamily } from '@/data/mock/family.mock';
import type { Paginated } from '@/types/pagination';
import { useFamilyStore } from '@/stores/family.store';
import { simulateLatency } from '@/utils/mockUtils';
import { createServices } from '@/services/service.factory';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

import { fixedMockFamilies } from '@/data/mock/fixed.family.mock';

const TOTAL_ITEMS = fixedMockFamilies.length;
const ITEMS_PER_PAGE = DEFAULT_ITEMS_PER_PAGE;

// Create a mock service for testing
class MockFamilyServiceForTest implements IFamilyService {
  public _items: Family[] = [];
  public shouldThrowError: boolean = false;

  constructor() {
    this.reset();
  }

  reset() {
    this._items = [...fixedMockFamilies]; // Use a copy of fixed mock families
    this.shouldThrowError = false;
  }

  // Getter to return a copy of the items array
  get items(): Family[] {
    return [...this._items]; // Return a shallow copy
  }

  async fetch(): Promise<Family[]> {
    if (this.shouldThrowError) {
      throw new Error('Mock fetch error');
    }
    return simulateLatency(this.items);
  }
  async getById(id: string): Promise<Family | undefined> {
    if (this.shouldThrowError) {
      throw new Error('Mock getById error');
    }
    return simulateLatency(this.items.find((f) => f.id === id));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Family> {
    if (this.shouldThrowError) {
      throw new Error('Mock add error');
    }
    const familyToAdd = { ...newItem, id: generateMockFamily().id };
    this._items.push(familyToAdd);
    return simulateLatency(familyToAdd);
  }
  async update(updatedItem: Family): Promise<Family> {
    if (this.shouldThrowError) {
      throw new Error('Mock update error');
    }
    const index = this._items.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this._items[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Family not found');
  }
  async delete(id: string): Promise<void> {
    if (this.shouldThrowError) {
      throw new Error('Mock delete error');
    }
    const initialLength = this._items.length;
    this._items = this._items.filter((f) => f.id !== id);
    if (this._items.length === initialLength) {
      throw new Error('Family not found');
    }
    return simulateLatency(undefined);
  }

  async searchItems(
    filter: FamilySearchFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Paginated<Family>> {
    if (this.shouldThrowError) {
      throw new Error('Mock searchItems error');
    }
    let filtered = this._items;

    // Filter by name
    if (filter.name) {
      const lowerCaseName = filter.name.toLowerCase();
      filtered = filtered.filter(
        (family) => family.name.toLowerCase().includes(lowerCaseName),
      );
    }

    // Filter by description
    if (filter.description) {
      const lowerCaseDescription = filter.description.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.description &&
          family.description.toLowerCase().includes(lowerCaseDescription),
      );
    }

    // Filter by address
    if (filter.address) {
      const lowerCaseAddress = filter.address.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.address && family.address.toLowerCase().includes(lowerCaseAddress),
      );
    }

    // Filter by visibility
    if (filter.visibility && filter.visibility !== 'all') {
      filtered = filtered.filter(
        (family) => family.visibility === filter.visibility,
      );
    }

    // Filter by createdAtStart
    if (filter.createdAtStart) {
      filtered = filtered.filter(
        (family) =>
          family.createdAt && family.createdAt >= filter.createdAtStart!,
      );
    }

    // Filter by createdAtEnd
    if (filter.createdAtEnd) {
      filtered = filtered.filter(
        (family) =>
          family.createdAt && family.createdAt <= filter.createdAtEnd!,
      );
    }

    // Filter by updatedAtStart
    if (filter.updatedAtStart) {
      filtered = filtered.filter(
        (family) =>
          family.updatedAt && family.updatedAt >= filter.updatedAtStart!,
      );
    }

    // Filter by updatedAtEnd
    if (filter.updatedAtEnd) {
      filtered = filtered.filter(
        (family) =>
          family.updatedAt && family.updatedAt <= filter.updatedAtEnd!,
      );
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

  beforeEach(() => {
    mockFamilyService = new MockFamilyServiceForTest();
    mockFamilyService.reset(); // Reset the mock service state
    const pinia = createPinia();
    setActivePinia(pinia);
    const store = useFamilyStore(); // Get the store instance

    store.$reset(); // Reset store state before each test

    store.services = createServices('test', {
      family: mockFamilyService,
    });
  });

  // 1. Initial state
  it('should have correct initial state', () => {
    const store = useFamilyStore();
    expect(store.items).toEqual([]);
    expect(store.currentItem).toBeNull();
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.totalItems).toBe(0);
    expect(store.currentPage).toBe(1);
    expect(store.totalPages).toBe(1);
    expect(store.itemsPerPage).toBe(DEFAULT_ITEMS_PER_PAGE);
  });

  // 2. Fetch families
  it('_loadItems should populate items array, totalItems, and totalPages on success', async () => {
    const store = useFamilyStore();
    await store._loadItems();
    expect(store.items.length).toBe(ITEMS_PER_PAGE); // Assuming initial load fetches first page
    expect(store.totalItems).toBe(TOTAL_ITEMS);
    expect(store.items[0].name).toBe(mockFamilyService.items[0].name);
    expect(store.loading).toBe(false);
    expect(store.totalPages).toBe(Math.ceil(TOTAL_ITEMS / ITEMS_PER_PAGE));
    expect(store.error).toBeNull();
  });

  it('_loadItems should set error and loading to false on fetch failure', async () => {
    mockFamilyService.shouldThrowError = true;
    const store = useFamilyStore();
    await store._loadItems();
    expect(store.error).toBe('Không thể tải danh sách gia đình.');
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]); // Items should be empty on error
  });

  // 3. Get by ID
  it('getItemById should return the correct family when found', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure families are loaded
    const family = store.getItemById(mockFamilyService.items[0].id);
    expect(family).toBeDefined();
    expect(family?.name).toBe(mockFamilyService.items[0].name);
  });

  it('getItemById should return undefined when family is not found', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure families are loaded
    const family = store.getItemById('non-existent-id');
    expect(family).toBeUndefined();
  });

  // 4. Add family
  it('addItem should add a new family and update state on success', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
    const initialTotalItems = store.totalItems;
    const newFamilyData: Omit<Family, 'id'> = {
      name: 'The New Family',
      description: 'A newly added family.',
      avatarUrl: 'test-avatar.jpg',
      address: 'test-address',
      visibility: 'public',
    };
    await store.addItem(newFamilyData);
    expect(store.totalItems).toBe(initialTotalItems + 1);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();

    // Verify the new family can be found by searching for it
    await store.searchItems({ name: newFamilyData.name });
    expect(store.items.length).toBe(1);
    expect(store.items[0].name).toBe(newFamilyData.name);
  });

  it('addItem should set error and not change families on add failure', async () => {
    mockFamilyService.shouldThrowError = true;
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
    const initialFamiliesCount = store.items.length;
    const initialTotalItems = store.totalItems;
    const newFamilyData: Omit<Family, 'id'> = {
      name: 'The Error Family',
      description: 'This family should not be added.',
      avatarUrl: 'test-avatar.jpg',
      address: 'test-address',
      visibility: 'public',
    };
    await store.addItem(newFamilyData);
    expect(store.error).toBe('Không thể thêm gia đình.');
    expect(store.loading).toBe(false);
    expect(store.items.length).toBe(initialFamiliesCount);
    expect(store.totalItems).toBe(initialTotalItems);
  });

  // 5. Update family
  it('updateItem should update an existing family and state on success', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
    const familyToUpdate = store.items[0];
    if (familyToUpdate) {
      const updatedName = 'The Updated Family';
      const updatedFamily: Family = { ...familyToUpdate, name: updatedName };
      await store.updateItem(updatedFamily);
      const foundFamily = store.getItemById(familyToUpdate.id);
      expect(foundFamily?.name).toBe(updatedName);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
    } else {
      expect.fail('No family to update.');
    }
  });

  it('updateItem should set error when family not found or on update failure', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
    const initialFamilies = [...store.items];
    const nonExistentFamily: Family = {
      id: 'non-existent-id',
      name: 'Non Existent',
      description: 'Should not be updated',
      avatarUrl: 'test-avatar.jpg',
      address: 'test-address',
      visibility: 'private',
    };

    // Test case 1: Family not found (mock service throws error)
    await store.updateItem(nonExistentFamily);
    expect(store.error).toBe('Không thể cập nhật gia đình.'); // This is the store's error
    expect(store.loading).toBe(false);
    expect(store.items).toEqual(initialFamilies); // Families should not change

    // Reset error and simulate service error
    store.error = null;
    mockFamilyService.shouldThrowError = true;
    const familyToUpdate = store.items[0];
    if (familyToUpdate) {
      const updatedFamily: Family = { ...familyToUpdate, name: 'Error Update' };
      await store.updateItem(updatedFamily);
      expect(store.error).toBe('Không thể cập nhật gia đình.'); // This is the store's error
      expect(store.loading).toBe(false);
      expect(store.items).toEqual(initialFamilies); // Families should not change
    } else {
      expect.fail('No family to update for service error test.');
    }
  });

  // 6. Delete family
  it('deleteItem should remove a family and update state on success', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
    const initialFamiliesCount = store.items.length;
    const initialTotalItems = store.totalItems;
    const familyToDeleteId = store.items[0]?.id;
    if (familyToDeleteId) {
      await store.deleteItem(familyToDeleteId);
      expect(store.totalItems).toBe(initialTotalItems - 1);
      expect(store.getItemById(familyToDeleteId)).toBeUndefined();
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
    } else {
      expect.fail('No family to delete.');
    }
  });

  it('deleteItem should set error and not change families on delete failure', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch to populate items
    const initialFamilies = [...store.items];
    const familyToDeleteId = store.items[0]?.id;
    if (familyToDeleteId) {
      mockFamilyService.shouldThrowError = true; // Set to true only for the delete call
      await store.deleteItem(familyToDeleteId);
      expect(store.error).toBe('Không thể xóa gia đình.');
      expect(store.loading).toBe(false);
      expect(store.items).toEqual(initialFamilies); // Families should not change
      mockFamilyService.shouldThrowError = false; // Reset after the call
    } else {
      expect.fail('No family to delete for error test.');
    }
  });

  // Edge case: Delete a non-existent ID
  it('deleteItem should set error when deleting a non-existent ID', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is populated
    const nonExistentId = 'non-existent-id';
    await store.deleteItem(nonExistentId);
    expect(store.error).toBe('Không thể xóa gia đình.');
    expect(store.loading).toBe(false);
  });

  // Edge case: Update with existing ID but no data change
  it('updateItem should not change state if data is identical', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
    const initialFamily = { ...store.items[0] }; // Deep copy
    if (initialFamily) {
      await store.updateItem(initialFamily);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.items[0]).toEqual(initialFamily);
    } else {
      expect.fail('No family to update for identical data test.');
    }
  });

  // 7. Search families (dùng SearchFilter + pagination)
  it('searchItems should filter, paginate, and update state correctly', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial load

    // Test 1: Search with query, page 1, itemsPerPage = 1
    const searchQuery = mockFamilyService.items[0].name;
    await store.searchItems({ name: searchQuery }); // Pass only filter object
    const expectedFilteredFamilies = mockFamilyService.items.filter(f =>
      f.name.toLowerCase().includes(searchQuery.toLowerCase()),
    );
    expect(store.items.length).toBe(Math.min(1, expectedFilteredFamilies.length));
    expect(store.totalItems).toBe(expectedFilteredFamilies.length);
    expect(store.currentPage).toBe(1);
    expect(store.filter.name).toBe(searchQuery); // Check filter.name
    expect(store.filter.visibility).toBeUndefined(); // Check filter.visibility
    expect(store.totalPages).toBe(Math.ceil(expectedFilteredFamilies.length / 1)); // Expect totalPages to be 1

    // Test 2: Change page
    if (store.totalPages > 1) {
      await store.setPage(2);
      expect(store.currentPage).toBe(2);
      // Verify families are different from page 1
      const familiesPage1 = expectedFilteredFamilies.slice(0, ITEMS_PER_PAGE);
      const familiesPage2 = expectedFilteredFamilies.slice(ITEMS_PER_PAGE, ITEMS_PER_PAGE * 2);
      expect(store.items.map(f => f.id)).toEqual(familiesPage2.map(f => f.id));
    }

    // Test 3: Change search query, should reset to page 1
    const newSearchQuery = mockFamilyService.items[1].name;
    await store.searchItems({ name: newSearchQuery }); // Pass only filter object
    expect(store.currentPage).toBe(1);
    expect(store.filter.name).toBe(newSearchQuery); // Check filter.name
    expect(store.totalItems).toBe(1); // Directly assert 1
    expect(store.totalPages).toBe(1); // Directly assert 1

    // Test 4: Filter by visibility
    await store.searchItems({ visibility: 'public' }); // Pass only filter object
    const publicFamilies = mockFamilyService.items.filter(f => f.visibility === 'public');
    expect(store.totalItems).toBe(publicFamilies.length);
    expect(store.items.every(f => f.visibility === 'public')).toBe(true);
    expect(store.currentPage).toBe(1);
    expect(store.filter.visibility).toBe('public');
  });

  // 8. Pagination edge cases
  it('currentPage should adjust to last page if totalPages decreases after delete', async () => {
    const store = useFamilyStore();
    // Set up a scenario where deleting one item reduces totalPages
    // For example, if we have 11 items and ITEMS_PER_PAGE = 10, totalPages = 2.
    // Deleting one item makes it 10 items, totalPages = 1. currentPage should go from 2 to 1.
    mockFamilyService.reset(); // Reset to default 50 items
    mockFamilyService._items = [...fixedMockFamilies.slice(0, ITEMS_PER_PAGE + 1)]; // 11 items
    await store._loadItems(); // totalItems = 11, totalPages = 2
    expect(store.totalItems).toBe(ITEMS_PER_PAGE + 1);
    expect(store.totalPages).toBe(2);

    await store.setPage(2);
    expect(store.currentPage).toBe(2);

    // Delete an item, reducing totalItems to 10, totalPages to 1
    const familyToDeleteId = store.items[0]?.id;
    if (familyToDeleteId) {
      await store.deleteItem(familyToDeleteId);
      expect(store.totalItems).toBe(ITEMS_PER_PAGE);
      expect(store.totalPages).toBe(1);
      expect(store.currentPage).toBe(1); // Should adjust to 1
    } else {
      expect.fail('No family to delete for pagination edge case.');
    }
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // TOTAL_ITEMS items, ITEMS_PER_PAGE per page, 5 pages

    expect(store.itemsPerPage).toBe(ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(5);

    // Change to 3 items per page
    await store.setItemsPerPage(3);
    expect(store.itemsPerPage).toBe(3);
    expect(store.totalPages).toBe(Math.ceil(TOTAL_ITEMS / 3));
    expect(store.currentPage).toBe(1); // Should reset to 1

    // Change back to default items per page
    await store.setItemsPerPage(ITEMS_PER_PAGE);
    expect(store.itemsPerPage).toBe(ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(5);
    expect(store.currentPage).toBe(1); // Should remain 1
  });

  // 9. Error handling in search
  it('searchItems should set error and clear items on search failure', async () => {
    mockFamilyService.shouldThrowError = true;
    const store = useFamilyStore();
    await store._loadItems(); // Initial load to populate items
    const initialItems = [...store.items];

    await store.searchItems({ name: 'non-existent-query' });
    expect(store.error).toBe('Không thể tải danh sách gia đình.');
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]); // Items should be empty on error
    expect(store.totalItems).toBe(0);
    expect(store.totalPages).toBe(1);
  });

  // 10. Computed getters
  it('paginatedItems should return families for the current page and itemsPerPage', async () => {
    const store = useFamilyStore();
    store.items = [...mockFamilyService.items]; // Manually set items for debugging
    store.totalItems = TOTAL_ITEMS;
    store.totalPages = Math.ceil(TOTAL_ITEMS / ITEMS_PER_PAGE);
    store.currentPage = 1; // Explicitly set
    store.itemsPerPage = ITEMS_PER_PAGE; // Explicitly set

    expect(store.items.length).toBe(TOTAL_ITEMS); // Explicitly check store.items length

    // Default page 1, ITEMS_PER_PAGE
    expect(store.paginatedItems.length).toBe(ITEMS_PER_PAGE);
    expect(store.paginatedItems[0].id).toBe(mockFamilyService.items[0].id);
  });

  it('setCurrentItem should set the current family', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is initialized
    const mockFamily: Family = generateMockFamily('test-id');
    store.setCurrentItem(mockFamily);
    expect(store.currentItem).toEqual(mockFamily);
    expect(store.currentItem).toBeDefined();
    expect(store.currentItem?.id).toBe(mockFamily.id);

    store.setCurrentItem(null);
    expect(store.currentItem).toBeNull();
  });

  it('setPage should update currentPage and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // TOTAL_ITEMS items, ITEMS_PER_PAGE per page, 5 pages

    await store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(store.items.length).toBe(ITEMS_PER_PAGE); // Second page of ITEMS_PER_PAGE items
    expect(store.items[0]?.id).toBe(mockFamilyService.items[ITEMS_PER_PAGE].id); // First item of second page

    // Invalid page (too high)
    await store.setPage(99);
    expect(store.currentPage).toBe(2); // Should remain 2

    // Invalid page (too low) - currentPage should not change
    await store.setPage(0);
    expect(store.currentPage).toBe(2); // Should remain 2
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and re-load items', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // TOTAL_ITEMS items, ITEMS_PER_PAGE per page, 5 pages

    expect(store.itemsPerPage).toBe(ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(5);

    // Change to 3 items per page
    await store.setItemsPerPage(3);
    expect(store.itemsPerPage).toBe(3);
    expect(store.totalPages).toBe(Math.ceil(TOTAL_ITEMS / 3));
    expect(store.currentPage).toBe(1); // Should reset to 1

    // Change back to default items per page
    await store.setItemsPerPage(ITEMS_PER_PAGE);
    expect(store.itemsPerPage).toBe(ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(5);
    expect(store.currentPage).toBe(1); // Should remain 1
  });
});