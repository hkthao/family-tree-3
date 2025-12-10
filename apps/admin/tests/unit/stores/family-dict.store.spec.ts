import { setActivePinia, createPinia } from 'pinia';
import { useFamilyDictStore } from '@/stores/family-dict.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, FamilyDict } from '@/types';
import { ok, err, FamilyDictLineage, FamilyDictType } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock the IFamilyDictService
const mockFetch = vi.fn();
const mockGetById = vi.fn();
const mockAdd = vi.fn();
const mockUpdate = vi.fn();
const mockDelete = vi.fn();
const mockLoadItems = vi.fn();
const mockGetByIds = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    familyDict: {
      fetch: mockFetch,
      getById: mockGetById,
      add: mockAdd,
      update: mockUpdate,
      delete: mockDelete,
      loadItems: mockLoadItems,
      getByIds: mockGetByIds,
    },
    // Add other services as empty objects if they are not directly used by family-dict.store
    ai: {},
    auth: {},
    chat: {},
    event: {},
    face: {},
    faceMember: {},
    fileUpload: {},
    family: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('family-dict.store', () => {
  let store: ReturnType<typeof useFamilyDictStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFamilyDictStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');
    // Reset mocks before each test
    mockFetch.mockReset();
    mockGetById.mockReset();
    mockAdd.mockReset();
    mockUpdate.mockReset();
    mockDelete.mockReset();
    mockLoadItems.mockReset();
    mockGetByIds.mockReset();

    // Default mock resolved values
    mockAdd.mockResolvedValue(ok(mockFamilyDict));
    mockDelete.mockResolvedValue(ok(undefined));
    mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilyDicts));
  });

  const mockFamilyDict: FamilyDict = {
    id: 'family-dict-1',
    name: 'Test FamilyDict',
    description: 'A family dict for testing',
    type: FamilyDictType.Blood,
    lineage: FamilyDictLineage.Noi,
    specialRelation: false,
    namesByRegion: {
      north: 'Bac',
      central: 'Trung',
      south: 'Nam',
    },
  };

  const mockPaginatedFamilyDicts = {
    items: [mockFamilyDict],
    page: 1,
    totalItems: 1, // Changed from totalCount
    totalPages: 1, // Added
  };

  // --- Actions Tests ---

  describe('_loadItems', () => {
    it('should load items successfully', async () => {
      mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilyDicts));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.list.items).toEqual([mockFamilyDict]);
      expect(store.list.totalItems).toBe(1);
      expect(store.list.totalPages).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle load items failure', async () => {
      const errorMessage = 'Failed to load family dicts.';
      mockLoadItems.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.list.items).toEqual([]);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItem', () => {
    it('should add an item successfully', async () => {
      mockAdd.mockResolvedValue(ok(mockFamilyDict));
      const newItem = { ...mockFamilyDict };

      const result = await store.addItem(newItem);

      expect(result.ok).toBe(true);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle add item failure', async () => {
      const errorMessage = 'Failed to add family dict.';
      mockAdd.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const newItem = { ...mockFamilyDict };

      const result = await store.addItem(newItem);

      expect(result.ok).toBe(false);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('updateItem', () => {
    it('should update an item successfully', async () => {
      mockUpdate.mockResolvedValue(ok(mockFamilyDict));
      mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilyDicts));

      await store.updateItem(mockFamilyDict);

      expect(store.update.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle update item failure', async () => {
      const errorMessage = 'Failed to update family dict.';
      mockUpdate.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.updateItem(mockFamilyDict);

      expect(store.update.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('deleteItem', () => {
    it('should delete an item successfully', async () => {
      mockDelete.mockResolvedValue(ok(undefined));
      mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilyDicts));

      const result = await store.deleteItem(mockFamilyDict.id!);

      expect(result.ok).toBe(true);
      expect(store._delete.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle delete item failure', async () => {
      const errorMessage = 'Failed to delete family dict.';
      mockDelete.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.deleteItem(mockFamilyDict.id!);

      expect(result.ok).toBe(false);
      expect(store._delete.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('getById', () => {
    it('should get an item by ID successfully', async () => {
      mockGetById.mockResolvedValue(ok(mockFamilyDict));

      const result = await store.getById(mockFamilyDict.id!);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toEqual(mockFamilyDict);
      expect(result).toEqual(mockFamilyDict);
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });

    it('should handle get by ID failure', async () => {
      const errorMessage = 'Failed to load family dict.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getById(mockFamilyDict.id!);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });
  });

  describe('getByIds', () => {
    it('should get items by IDs successfully', async () => {
      mockGetByIds.mockResolvedValue(ok([mockFamilyDict]));

      const result = await store.getByIds([mockFamilyDict.id!]);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result.ok).toBe(true);
      if (result.ok) {
        expect(result.value).toEqual([mockFamilyDict]);
      }
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });

    it('should handle get by IDs failure', async () => {
      const errorMessage = 'Failed to load family dicts.';
      mockGetByIds.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getByIds([mockFamilyDict.id!]);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(result.ok).toBe(false);
      if (!result.ok) {
        expect(result.error).toBeTruthy();
      }
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });
  });

  // describe('setPage', () => { /* ... removed ... */ });
  // describe('setItemsPerPage', () => { /* ... removed ... */ });
  // describe('setSortBy', () => { /* ... removed ... */ });

  describe('setListOptions', () => {
    it('should set list options and load items', async () => {
      const options = {
        page: 2,
        itemsPerPage: 20,
        sortBy: [{ key: 'name', order: 'desc' }],
      };
      mockLoadItems.mockResolvedValue(ok({ ...mockPaginatedFamilyDicts, page: options.page }));
      await store.setListOptions(options);

      expect(store.list.currentPage).toBe(options.page);
      expect(store.list.itemsPerPage).toBe(20);
      expect(store.list.sortBy).toEqual([{ key: 'name', order: 'desc' }]);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });
  });

  describe('setCurrentItem', () => {
    it('should set the current item', async () => {
      await store.setCurrentItem(mockFamilyDict);
      expect(store.detail.item).toEqual(mockFamilyDict);
    });
  });

  describe('clearItems', () => {
    it('should clear the list items and reset pagination', () => {
      store.list.items = [mockFamilyDict];
      store.list.totalItems = 1;
      store.list.totalPages = 1;
      store.list.currentPage = 1;

      store.clearItems();

      expect(store.list.items).toEqual([]);
      expect(store.list.totalItems).toBe(0);
      expect(store.list.totalPages).toBe(1);
      expect(store.list.currentPage).toBe(1);
    });
  });
});