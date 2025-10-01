import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import type { IFamilyService } from '@/services/family/family.service.interface';
import type { ApiError } from '@/utils/api';
import { useFamilyStore } from '@/stores/family.store';
import { simulateLatency } from '@/utils/mockUtils';
import { createServices } from '@/services/service.factory';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import {
  type Family,
  type Result,
  ok,
  err,
  FamilyVisibility,
  type FamilyFilter,
  type Paginated,
} from '@/types';

import fixedMockFamilies from '@/data/mock/families.json';

// Create a mock service for testing
class MockFamilyServiceForTest implements IFamilyService {
  public items: Family[] = JSON.parse(JSON.stringify(fixedMockFamilies));
  public shouldThrowError: boolean = false;
  public errorType:
    | 'load'
    | 'add'
    | 'update'
    | 'delete'
    | 'getById'
    | 'getByIds'
    | null = null;

  reset() {
    this.items = JSON.parse(JSON.stringify(fixedMockFamilies));
    this.shouldThrowError = false;
    this.errorType = null;
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
    const familyToAdd = { ...newItem, id: new Date().getTime().toString() };
    this.items.push(familyToAdd);
    return ok(await simulateLatency(familyToAdd));
  }

  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock update error' });
    }
    const index = this.items.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this.items[index] = updatedItem;
      return ok(await simulateLatency(updatedItem));
    }
    return err({ message: 'Family not found', statusCode: 404 });
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock delete error' });
    }
    const initialLength = this.items.length;
    this.items = this.items.filter((f) => f.id !== id);
    if (this.items.length === initialLength) {
      return err({ message: 'Family not found', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }

  async loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Mock loadItems error' });
    }
    let filtered = [...this.items];

    // Filter by name
    if (filter.searchQuery) {
      const lowerCaseName = filter.searchQuery.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseName) ||
          (family.description || '').toLowerCase().includes(lowerCaseName) ||
          (family.address || '').toLowerCase().includes(lowerCaseName),
      );
    }

    // Filter by visibility
    if (filter.visibility && filter.visibility !== 'all') {
      filtered = filtered.filter(
        (family) => family.visibility === filter.visibility,
      );
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return ok(
      await simulateLatency({
        items,
        totalItems,
        totalPages,
      }),
    );
  }

  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    const families = this.items.filter((f) => ids.includes(f.id));
    return ok(await simulateLatency(families));
  }
}

const createStore = (
  shouldThrowError: boolean = false,
  errorType: MockFamilyServiceForTest['errorType'] = null,
) => {
  const mockFamilyService = new MockFamilyServiceForTest();
  mockFamilyService.shouldThrowError = shouldThrowError;
  mockFamilyService.errorType = errorType;
  const store = useFamilyStore();
  store.services = createServices('test', { family: mockFamilyService });
  return store;
};

describe('Family Store', () => {
  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
  });

  // 1. Initial state
  it('should have correct initial state', () => {
    const store = createStore();
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
    const store = createStore();
    await store._loadItems();
    expect(store.totalItems).toBe(50); // Assuming initial load fetches first page
    expect(store.totalPages).toBe(5); // Assuming initial load fetches first page
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  it('_loadItems should set error and loading to false on fetch failure', async () => {
    const store = createStore(true, 'load');
    await store._loadItems();
    expect(store.error?.length).toBeGreaterThan(0);
    expect(store.loading).toBe(false);
    expect(store.totalItems).toBe(0);
  });

  // 3. Get by ID
  it('getById should return the correct family when found', async () => {
    const store = createStore();
    await store._loadItems(); // Ensure families are loaded
    await store.getById(store.items[0].id);
    expect(store.currentItem).toBeDefined();
    expect(store.currentItem?.name).toBe(store.items[0].name);
  });

  it('getById should return undefined when family is not found', async () => {
    const store = createStore();
    await store._loadItems();
    await store.getById('non-existent-id');
    expect(store.currentItem).toBeNull();
  });

  // 4. Add family
  it('addItem should add a new family and update state on success', async () => {
    const store = createStore();
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
  });

  it('addItem should set error and not change families on add failure', async () => {
    const store = createStore(true, 'add');
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
    expect(store.error?.length).toBeGreaterThan(0);
    expect(store.loading).toBe(false);
    expect(store.items.length).toBe(initialFamiliesCount);
    expect(store.totalItems).toBe(initialTotalItems);
  });

  it('addItem should set generic error message when result.error.message is undefined', async () => {
    const store = createStore(true, 'add');
    await store._loadItems();
    const initialTotalItems = store.totalItems;
    const newFamilyData: Omit<Family, 'id'> = {
      name: 'The Error Family',
      description: 'This family should not be added.',
      avatarUrl: 'test-avatar.jpg',
      address: 'test-address',
      visibility: FamilyVisibility.Public,
    };
    await store.addItem(newFamilyData);
    expect(store.error?.length).toBeGreaterThan(0);
    expect(store.loading).toBe(false);
    expect(store.totalItems).toBe(initialTotalItems);
  });

  // 5. Update family
  it('updateItem should update an existing family and state on success', async () => {
    const store = createStore();
    await store._loadItems();
    const familyToUpdate = store.items[0];
    const updatedName = 'The Updated Family';
    const updatedFamily: Family = { ...familyToUpdate, name: updatedName };
    await store.updateItem(updatedFamily);
    await store.getById(familyToUpdate.id);
    const foundFamily = store.currentItem;
    expect(foundFamily?.name).toBe(updatedName);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  it('updateItem should set error when family not found', async () => {
    const store = createStore();
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

    await store.updateItem(nonExistentFamily);
    expect(store.error?.length).toBeGreaterThan(0);
    expect(store.loading).toBe(false);
    expect(store.items).toEqual(initialFamilies); // Families should not change
  });

  // 6. Delete family
  it('deleteItem should remove a family and update state on success', async () => {
    const store = createStore();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store._loadItems(); // Initial fetch
    const initialTotalItems = store.totalItems;
    const familyToDeleteId = store.items[0]?.id;
    await store.deleteItem(familyToDeleteId);
    expect(store.totalItems).toBe(initialTotalItems - 1);
    expect(loadItemsSpy).toHaveBeenCalled();
    await store.getById(familyToDeleteId);
    expect(store.currentItem).toBeNull();
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  it('deleteItem should set error and not change families on delete failure', async () => {
    const store = createStore(true);
    await store._loadItems(); // Initial fetch to populate items
    const initialFamilies = [...store.items];
    const familyToDeleteId = store.items[0]?.id;
    await store.deleteItem(familyToDeleteId);
    expect(store.error?.length).toBeGreaterThan(0);
    expect(store.loading).toBe(false);
    expect(store.items).toEqual(initialFamilies); // Families should not change
  });

  it('deleteItem should set error when deleting a non-existent ID', async () => {
    const store = createStore(true);
    await store._loadItems();
    const initialFamilies = [...store.items];
    const nonExistentId = 'non-existent-id';
    await store.deleteItem(nonExistentId);
    expect(store.error?.length).toBeGreaterThan(0);
    expect(store.loading).toBe(false);
    expect(store.items).toEqual(initialFamilies); // Families should not change
  });

  it('setItemsPerPage should update itemsPerPage, reset currentPage, and re-load items', async () => {
    const store = createStore();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    expect(store.itemsPerPage).toBe(DEFAULT_ITEMS_PER_PAGE);
    expect(store.totalPages).toBe(1);
    expect(store.currentPage).toBe(1);
    // Change to 3 items per page
    await store.setItemsPerPage(3);
    expect(store.itemsPerPage).toBe(3);
    expect(store.currentPage).toBe(1);
    expect(loadItemsSpy).toHaveBeenCalled();
  });

  // 9. Error handling in search
  it('loadItems should set error and clear items on search failure', async () => {
    const store = createStore(true, 'load');
    await store._loadItems(); // Initial load to populate items
    expect(store.loading).toBe(false);
    expect(store.items).toEqual([]); // Items should be empty on error
    expect(store.totalItems).toBe(0);
    expect(store.totalPages).toBe(1);
  });

  it('getById should return undefined and log error if service call fails', async () => {
    const store = createStore();
    await store.getById('non-existent-id');
    expect(store.currentItem).toBeNull();
  });
});
