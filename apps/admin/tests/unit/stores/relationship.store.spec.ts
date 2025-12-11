import { setActivePinia, createPinia } from 'pinia';
import { useRelationshipStore } from '@/stores/relationship.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Relationship, Paginated, ApiError } from '@/types';
import { RelationshipType } from '@/types';
import { ok, err } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock the IRelationshipService
const mockFetch = vi.fn();
const mockGetById = vi.fn();
const mockAdd = vi.fn();
const mockUpdate = vi.fn();
const mockDelete = vi.fn();
const mockSearch = vi.fn();
const mockGetByIds = vi.fn(); // Keep this mock as it's used in the mocked service factory
const mockAddItems = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    relationship: {
      fetch: mockFetch,
      getById: mockGetById,
      add: mockAdd,
      update: mockUpdate,
      delete: mockDelete,
      search: mockSearch,
      getByIds: mockGetByIds, // Keep this here for the mock
      addItems: mockAddItems,
    },
    // Add other services as empty objects if they are not directly used by relationship.store
    // This prevents 'Cannot read properties of undefined' errors if other stores are initialized
    // and try to access services that are not mocked.
    ai: {},
    auth: {},
    chat: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    family: {},
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('relationship.store', () => {
  let store: ReturnType<typeof useRelationshipStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useRelationshipStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');
    // Reset mocks before each test
    mockFetch.mockReset();
    mockGetById.mockReset();
    mockAdd.mockReset();
    mockUpdate.mockReset();
    mockDelete.mockReset();
    mockSearch.mockReset();
    mockGetByIds.mockReset();
    mockAddItems.mockReset();

    mockAdd.mockResolvedValue(ok(mockRelationship)); // Default for addItem
    mockUpdate.mockResolvedValue(ok(mockRelationship)); // Default for updateItem
    mockDelete.mockResolvedValue(ok(undefined)); // Default for deleteItem
    mockAddItems.mockResolvedValue(ok(['new-id-1'])); // Default for addItems
    // mockSearch.mockResolvedValue(ok(mockPaginatedRelationships)); // Moved to specific tests
  });

  const mockRelationship: Relationship = {
    id: 'rel-1',
    sourceMemberId: 'member-1',
    targetMemberId: 'member-2',
    type: RelationshipType.Husband,
    familyId: 'family-1',
    sourceMember: { fullName: 'Member One' },
    targetMember: { fullName: 'Member Two' },
  };

  const mockPaginatedRelationships: Paginated<Relationship> = {
    items: [mockRelationship],
    totalItems: 1,
    totalPages: 1,
    page: 1
  };

  // --- Actions Tests ---

  describe('_loadItems', () => {
    it('should load items successfully', async () => {
      mockSearch.mockResolvedValue(ok(mockPaginatedRelationships));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.list.items).toEqual([mockRelationship]);
      expect(store.list.totalPages).toEqual(1);
      expect(store.list.totalItems).toEqual(1); // Corrected from 0
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle load items failure', async () => {
      const errorMessage = 'Failed to load relationships.';
      mockSearch.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.list.items).toEqual([]);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItem', () => {
    it('should add an item successfully', async () => {
      mockSearch.mockResolvedValue(ok(mockPaginatedRelationships)); // _loadItems is called after successful add
      await store.addItem({ ...mockRelationship });

      expect(store.add.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeNull();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle add item failure', async () => {
      const errorMessage = 'Failed to add relationship.';
      mockAdd.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.addItem({ ...mockRelationship });

      expect(store.add.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeTruthy();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('updateItem', () => {
    it('should update an item successfully', async () => {
      mockSearch.mockResolvedValue(ok(mockPaginatedRelationships));

      await store.updateItem(mockRelationship);

      expect(store.update.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeNull();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle update item failure', async () => {
      const errorMessage = 'Failed to update relationship.';
      mockUpdate.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.updateItem(mockRelationship);

      expect(store.update.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeTruthy();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('deleteItem', () => {
    it('should delete an item successfully', async () => {
      mockSearch.mockResolvedValue(ok(mockPaginatedRelationships));

      const result = await store.deleteItem(mockRelationship.id!);

      expect(result.ok).toBe(true);
      expect(store._delete.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeNull();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle delete item failure', async () => {
      const errorMessage = 'Failed to delete relationship.';
      mockDelete.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.deleteItem(mockRelationship.id!);

      expect(result.ok).toBe(false);
      expect(store._delete.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeTruthy();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });

  describe('getById', () => {
    it('should get an item by ID successfully', async () => {
      mockGetById.mockResolvedValue(ok(mockRelationship));

      const result = await store.getById(mockRelationship.id!);

      expect(store.detail.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeNull();
      expect(store.detail.item).toEqual(mockRelationship); // Corrected from store.currentItem
      expect(result).toEqual(mockRelationship);
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });

    it('should handle get by ID failure', async () => {
      const errorMessage = 'Failed to load relationship.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getById(mockRelationship.id!);

      expect(store.detail.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeTruthy();
      expect(store.detail.item).toBeNull(); // Corrected from store.currentItem
      expect(result).toBeUndefined();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });
  });

  // Removed describe('getByIds', ...) block as the store does not have this method
  // The mockGetByIds is still in the service factory mock, which is fine.

  describe('addItems', () => {
    it('should add multiple items successfully', async () => {
      mockAddItems.mockResolvedValue(ok(['new-id-1', 'new-id-2']));
      mockSearch.mockResolvedValue(ok(mockPaginatedRelationships));

      const newRelationships = [
        { ...mockRelationship, id: undefined, sourceMemberId: 'm3', targetMemberId: 'm4' },
        { ...mockRelationship, id: undefined, sourceMemberId: 'm5', targetMemberId: 'm6' },
      ];
      const result = await store.addItems(newRelationships);

      expect(result.ok).toBe(true);
      expect(store.add.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeNull();
      expect(mockAddItems).toHaveBeenCalledWith(newRelationships);
      expect(mockSearch).toHaveBeenCalledTimes(1);
    });

    it('should handle add multiple items failure', async () => {
      const errorMessage = 'Failed to add multiple relationships.';
      mockAddItems.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const newRelationships = [
        { ...mockRelationship, id: undefined, sourceMemberId: 'm3', targetMemberId: 'm4' },
      ];
      const result = await store.addItems(newRelationships);

      expect(result.ok).toBe(false);
      expect(store.add.loading).toBe(false); // Corrected from store.loading
      expect(store.error).toBeTruthy();
      expect(mockAddItems).toHaveBeenCalledWith(newRelationships);
      expect(mockSearch).not.toHaveBeenCalled();
    });
  });
});