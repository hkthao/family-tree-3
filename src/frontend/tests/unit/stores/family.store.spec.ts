import { setActivePinia, createPinia } from 'pinia';
import { useFamilyStore } from '@/stores/family.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Family, Paginated } from '@/types';
import { FamilyVisibility } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IFamilyService
const mockFetch = vi.fn();
const mockGetById = vi.fn();
const mockAdd = vi.fn();
const mockUpdate = vi.fn();
const mockDelete = vi.fn();
const mockLoadItems = vi.fn();
const mockGetByIds = vi.fn();
const mockAddItems = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    family: {
      fetch: mockFetch,
      getById: mockGetById,
      add: mockAdd,
      update: mockUpdate,
      delete: mockDelete,
      loadItems: mockLoadItems,
      getByIds: mockGetByIds,
      addItems: mockAddItems,
    },
    // Add other services as empty objects if they are not directly used by family.store
    ai: {},
    auth: {},
    chat: {},
    chunk: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    fileUpload: {},
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

describe('family.store', () => {
  let store: ReturnType<typeof useFamilyStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFamilyStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('mock');
    // Reset mocks before each test
    mockFetch.mockReset();
    mockGetById.mockReset();
    mockAdd.mockReset();
    mockUpdate.mockReset();
    mockDelete.mockReset();
    mockLoadItems.mockReset();
    mockGetByIds.mockReset();
    mockAddItems.mockReset();
    mockAdd.mockResolvedValue(ok(mockFamily));
    mockDelete.mockResolvedValue(ok(undefined));
    mockAddItems.mockResolvedValue(ok(['new-id-1']));
    mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilies));
  });

  const mockFamily: Family = {
    id: 'family-1',
    name: 'Test Family',
    description: 'A family for testing',
    visibility: FamilyVisibility.Public,
  };

  const mockPaginatedFamilies: Paginated<Family> = {
    items: [mockFamily],
    totalItems: 1,
    totalPages: 1,
  };

  // --- Actions Tests ---

  describe('_loadItems', () => {
    it('should load items successfully', async () => {
      mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilies));

      await store._loadItems();

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.items).toEqual([mockFamily]);
      expect(store.totalItems).toBe(1);
      expect(store.totalPages).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle load items failure', async () => {
      const errorMessage = 'Failed to load families.';
      mockLoadItems.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store._loadItems();

      expect(store.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.items).toEqual([]);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItem', () => {
    it('should add an item successfully', async () => {
      mockAdd.mockResolvedValue(ok(mockFamily));
      const result = await store.addItem({ ...mockFamily });

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle add item failure', async () => {
      const errorMessage = 'Failed to add family.';
      mockAdd.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.addItem({ ...mockFamily });

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('updateItem', () => {
    it('should update an item successfully', async () => {
      mockUpdate.mockResolvedValue(ok(mockFamily));
      mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilies));

      await store.updateItem(mockFamily);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle update item failure', async () => {
      const errorMessage = 'Failed to update family.';
      mockUpdate.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.updateItem(mockFamily);

      expect(store.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('deleteItem', () => {
    it('should delete an item successfully', async () => {
      mockDelete.mockResolvedValue(ok(undefined));
      mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilies));

      const result = await store.deleteItem(mockFamily.id!);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle delete item failure', async () => {
      const errorMessage = 'Failed to delete family.';
      mockDelete.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.deleteItem(mockFamily.id!);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('getById', () => {
    it('should get an item by ID successfully', async () => {
      mockGetById.mockResolvedValue(ok(mockFamily));

      const result = await store.getById(mockFamily.id!);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.currentItem).toEqual(mockFamily);
      expect(result).toEqual(mockFamily);
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });

    it('should handle get by ID failure', async () => {
      const errorMessage = 'Failed to load family.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getById(mockFamily.id!);

      expect(store.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.currentItem).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });
  });

  describe('getByIds', () => {
    it('should get items by IDs successfully', async () => {
      mockGetByIds.mockResolvedValue(ok([mockFamily]));

      const result = await store.getByIds([mockFamily.id!]);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockFamily]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });

    it('should handle get by IDs failure', async () => {
      const errorMessage = 'Failed to load families.';
      mockGetByIds.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getByIds([mockFamily.id!]);

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(result).toEqual([]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItems', () => {
    it('should add multiple items successfully', async () => {
      mockAddItems.mockResolvedValue(ok(['new-id-1', 'new-id-2']));
      mockLoadItems.mockResolvedValue(ok(mockPaginatedFamilies));

      const newFamilies = [
        { name: 'Family 1', visibility: FamilyVisibility.Public },
        { name: 'Family 2', visibility: FamilyVisibility.Public },
      ];
      const result = await store.addItems(newFamilies);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAddItems).toHaveBeenCalledWith(newFamilies);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle add multiple items failure', async () => {
      const errorMessage = 'Failed to add multiple families.';
      mockAddItems.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const newFamilies = [
        { name: 'Family 1', visibility: FamilyVisibility.Public },
      ];
      const result = await store.addItems(newFamilies);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockAddItems).toHaveBeenCalledWith(newFamilies);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });
});