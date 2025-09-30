import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import type { Family, FamilySearchFilter } from '@/types/family';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { generateMockFamily } from '@/data/mock/family.mock';
import type { Paginated, Result } from '@/types/common';
import { ok, err } from '@/types/common';
import type { ApiError } from '@/utils/api';
import { useFamilyStore } from '@/stores/family.store';
import { simulateLatency } from '@/utils/mockUtils';
import { createServices } from '@/services/service.factory';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

import { fixedMockFamilies } from '@/data/mock/fixed.family.mock';
import { FamilyVisibility } from '@/types/family/family-visibility';

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

  async fetch(): Promise<Result<Family[], ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock fetch error' });
    }
    return ok(await simulateLatency(this.items));
  }
  async getById(id: string): Promise<Result<Family | undefined, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock getById error' });
    }
    return ok(await simulateLatency(this.items.find((f) => f.id === id)));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock add error' });
    }
    const familyToAdd = { ...newItem, id: generateMockFamily().id };
    this._items.push(familyToAdd);
    return ok(await simulateLatency(familyToAdd));
  }
  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock update error' });
    }
    const index = this._items.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this._items[index] = updatedItem;
      return ok(await simulateLatency(updatedItem));
    }
    return err({ message: 'Family not found', statusCode: 404 });
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock delete error' });
    }
    const initialLength = this._items.length;
    this._items = this._items.filter((f) => f.id !== id);
    if (this._items.length === initialLength) {
      return err({ message: 'Family not found', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }

  async loadItems(
    filter: FamilySearchFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock loadItems error' });
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

    return ok(await simulateLatency({
      items,
      totalItems,
      totalPages,
    }));
  }

  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    const families = this._items.filter(f => ids.includes(f.id));
    return ok(await simulateLatency(families));
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

    store.services = createServices('test', {
      family: mockFamilyService,
    });
    store.$reset(); // Call reset here
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
    expect(store.error).toBe('Mock loadItems error');
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]); // Items should be empty on error
  });

  it('_loadItems should set generic error message when result.error.message is undefined', async () => {
    mockFamilyService.shouldThrowError = true;
    // Mock the service to return an error without a message
    mockFamilyService.loadItems = vi.fn().mockResolvedValue(err({ message: undefined }));
    const store = useFamilyStore();
    await store._loadItems();
    expect(store.error).toBe('Không thể tải danh sách gia đình.');
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]);
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

  it('getItemById should return undefined when items array is empty', () => {
    const store = useFamilyStore();
    store.items = []; // Ensure items array is empty
    const family = store.getItemById('any-id');
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
      visibility: FamilyVisibility.Public,
    };
    await store.addItem(newFamilyData);
    expect(store.totalItems).toBe(initialTotalItems + 1);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();

    // Verify the new family can be found by searching for it
    await store.loadItems({ name: newFamilyData.name });
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
      visibility: FamilyVisibility.Public,
    };
    await store.addItem(newFamilyData);
    expect(store.error).toBe('Mock add error');
    expect(store.loading).toBe(false);
    expect(store.items.length).toBe(initialFamiliesCount);
    expect(store.totalItems).toBe(initialTotalItems);
  });

  it('addItem should set generic error message when result.error.message is undefined', async () => {
    mockFamilyService.shouldThrowError = true;
    mockFamilyService.add = vi.fn().mockResolvedValue(err({ message: undefined }));
    const store = useFamilyStore();
    await store._loadItems();
    const newFamilyData: Omit<Family, 'id'> = {
      name: 'The Error Family',
      description: 'This family should not be added.',
      avatarUrl: 'test-avatar.jpg',
      address: 'test-address',
      visibility: FamilyVisibility.Public,
    };
    await store.addItem(newFamilyData);
    expect(store.error).toBe('Không thể thêm gia đình.');
    expect(store.loading).toBe(false);
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
      visibility: FamilyVisibility.Private,
    };

    // Test case 1: Family not found (mock service throws error)
    await store.updateItem(nonExistentFamily);
    await expect(store.error).toBe('Family not found'); // This is the store's error
    expect(store.loading).toBe(false);
    expect(store.items).toEqual(initialFamilies); // Families should not change

    // Reset error and simulate service error
    store.error = null;
    mockFamilyService.shouldThrowError = true;
    const familyToUpdate = store.items[0];
    if (familyToUpdate) {
      const updatedFamily: Family = { ...familyToUpdate, name: 'Error Update' };
      await store.updateItem(updatedFamily);
      expect(store.error).toBe('Mock update error'); // This is the store's error
      expect(store.loading).toBe(false);
      expect(store.items).toEqual(initialFamilies); // Families should not change
    } else {
      expect.fail('No family to update for service error test.');
    }
  });

  it('updateItem should set generic error message when result.error.message is undefined', async () => {
    const store = useFamilyStore();
    await store._loadItems();
    mockFamilyService.shouldThrowError = true;
    mockFamilyService.update = vi.fn().mockResolvedValue(err({ message: undefined }));
    const familyToUpdate = store.items[0];
    if (familyToUpdate) {
      const updatedFamily: Family = { ...familyToUpdate, name: 'Error Update' };
      await store.updateItem(updatedFamily);
      expect(store.error).toBe('Không thể cập nhật gia đình.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No family to update for generic error test.');
    }
  });

  // 6. Delete family
  it('deleteItem should remove a family and update state on success', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial fetch
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
      expect(store.error).toBe('Mock delete error');
      expect(store.loading).toBe(false);
      expect(store.items).toEqual(initialFamilies); // Families should not change
      mockFamilyService.shouldThrowError = false; // Reset after the call
    } else {
      expect.fail('No family to delete for error test.');
    }
  });

  it('deleteItem should set generic error message when result.error.message is undefined', async () => {
    const store = useFamilyStore();
    await store._loadItems();
    mockFamilyService.shouldThrowError = true;
    mockFamilyService.delete = vi.fn().mockResolvedValue(err({ message: undefined }));
    const familyToDeleteId = store.items[0]?.id;
    if (familyToDeleteId) {
      await store.deleteItem(familyToDeleteId);
      expect(store.error).toBe('Không thể xóa gia đình.');
      expect(store.loading).toBe(false);
    } else {
      expect.fail('No family to delete for generic error test.');
    }
  });

  // Edge case: Delete a non-existent ID
  it('deleteItem should set error when deleting a non-existent ID', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is populated
    const nonExistentId = 'non-existent-id';
    await store.deleteItem(nonExistentId);
    expect(store.error).toBe('Family not found');
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
  it('loadItems should filter, paginate, and update state correctly', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Initial load

    // Test 1: Search with query, page 1, itemsPerPage = 1
    const searchQuery = mockFamilyService.items[0].name;
    await store.loadItems({ name: searchQuery }); // Pass only filter object
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
      const familiesPage2 = expectedFilteredFamilies.slice(ITEMS_PER_PAGE, ITEMS_PER_PAGE * 2);
      expect(store.items.map(f => f.id)).toEqual(familiesPage2.map(f => f.id));
    }

    // Test 3: Change search query, should reset to page 1
    const newSearchQuery = mockFamilyService.items[1].name;
    await store.loadItems({ name: newSearchQuery }); // Pass only filter object
    expect(store.currentPage).toBe(1);
    expect(store.filter.name).toBe(newSearchQuery); // Check filter.name
    expect(store.totalItems).toBe(1); // Directly assert 1
    expect(store.totalPages).toBe(1); // Directly assert 1

    // Test 4: Filter by visibility
    await store.loadItems({ visibility: FamilyVisibility.Public }); // Pass only filter object
    const publicFamilies = mockFamilyService.items.filter(f => f.visibility === FamilyVisibility.Public);
    expect(store.totalItems).toBe(publicFamilies.length);
    expect(store.items.every(f => f.visibility === FamilyVisibility.Public)).toBe(true);
    expect(store.currentPage).toBe(1);
    expect(store.filter.visibility).toBe(FamilyVisibility.Public);
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

  it('setItemsPerPage should not update itemsPerPage for invalid count values', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is populated
    const initialItemsPerPage = store.itemsPerPage;

    // Test with count <= 0
    await store.setItemsPerPage(0);
    expect(store.itemsPerPage).toBe(initialItemsPerPage);

    await store.setItemsPerPage(-5);
    expect(store.itemsPerPage).toBe(initialItemsPerPage);
  });

  // 9. Error handling in search
  it('loadItems should set error and clear items on search failure', async () => {
    mockFamilyService.shouldThrowError = true;
    const store = useFamilyStore();
    await store._loadItems(); // Initial load to populate items
    expect(store.error).toBe('Mock loadItems error');
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]); // Items should be empty on error
    expect(store.totalItems).toBe(0);
    expect(store.totalPages).toBe(1);
  });

  it('loadItems should set generic error message when result.error.message is undefined', async () => {
    mockFamilyService.shouldThrowError = true;
    mockFamilyService.loadItems = vi.fn().mockResolvedValue(err({ message: undefined }));
    const store = useFamilyStore();
    await store._loadItems();
    expect(store.error).toBe('Không thể tải danh sách gia đình.');
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]);
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

  it('paginatedItems should return an empty array when items is empty', () => {
    const store = useFamilyStore();
    store.items = [];
    expect(store.paginatedItems).toEqual([]);
  });

  it('getItemById should return item from cache if it exists', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is populated
    const cachedItem = mockFamilyService.items[0];
    store.itemCache[cachedItem.id] = cachedItem; // Manually add to cache

    const spy = vi.spyOn(mockFamilyService, 'getById');
    const fetchedItem = await store.getItemById(cachedItem.id);

    expect(fetchedItem).toEqual(cachedItem);
    expect(spy).not.toHaveBeenCalled(); // Should not call service if in cache
  });

  it('getItemById should fetch item from service and cache it if not in cache', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is populated
    const itemToFetch = mockFamilyService.items[0];
    store.itemCache = {}; // Clear cache

    const spy = vi.spyOn(mockFamilyService, 'getById');
    const fetchedItem = await store.getItemById(itemToFetch.id);

    expect(fetchedItem).toEqual(itemToFetch);
    expect(spy).toHaveBeenCalledWith(itemToFetch.id);
    expect(store.itemCache[itemToFetch.id]).toEqual(itemToFetch); // Should be cached
  });

  it('getItemById should return undefined and log error if service call fails', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is populated
    mockFamilyService.getById = vi.fn().mockResolvedValue(err({ message: 'Fetch error' }));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {}); // Mock console.error

    const fetchedItem = await store.getItemById('non-existent-id');

    expect(fetchedItem).toBeUndefined();
    expect(consoleSpy).toHaveBeenCalledWith('Error fetching item with ID non-existent-id:', { message: 'Fetch error' });
    consoleSpy.mockRestore(); // Restore console.error
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

  it('setPage should not update currentPage for invalid page values', async () => {
    const store = useFamilyStore();
    await store._loadItems(); // Ensure store is populated
    const initialPage = store.currentPage;

    // Test with page < 1
    await store.setPage(0);
    expect(store.currentPage).toBe(initialPage);

    // Test with page > totalPages
    await store.setPage(store.totalPages + 1);
    expect(store.currentPage).toBe(initialPage);
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