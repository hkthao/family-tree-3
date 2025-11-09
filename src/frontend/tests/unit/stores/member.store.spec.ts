import { setActivePinia, createPinia } from 'pinia';
import { useMemberStore } from '@/stores/member.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Member, Paginated } from '@/types';
import { Gender } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IMemberService
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
    member: {
      fetch: mockFetch,
      getById: mockGetById,
      add: mockAdd,
      update: mockUpdate,
      delete: mockDelete,
      loadItems: mockLoadItems,
      getByIds: mockGetByIds,
      addItems: mockAddItems,
    },
    // Add other services as empty objects if they are not directly used by member.store
    ai: {},
    auth: {},
    chat: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    family: {},
    fileUpload: {},
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

describe('member.store', () => {
  let store: ReturnType<typeof useMemberStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useMemberStore();
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
    mockAddItems.mockReset();

    mockAdd.mockResolvedValue(ok(mockMember));
    mockUpdate.mockResolvedValue(ok(mockMember));
    mockDelete.mockResolvedValue(ok(undefined));
    mockAddItems.mockResolvedValue(ok(['new-id-1']));
    // mockLoadItems.mockResolvedValue(ok(mockPaginatedMembers)); // Moved to specific tests
  });

  const mockMember: Member = {
    id: 'member-1',
    familyId: 'family-1',
    lastName: 'Test',
    firstName: 'Member',
    gender: Gender.Male,
    fullName: 'Test Member',
  };

  const mockPaginatedMembers: Paginated<Member> = {
    items: [mockMember],
    totalItems: 1,
    totalPages: 1,
    page: 1
  };

  // --- Actions Tests ---

  describe('_loadItems', () => {
    it('should load items successfully', async () => {
      mockLoadItems.mockResolvedValue(ok(mockPaginatedMembers));

      await store._loadItems();

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.list.items).toEqual([mockMember]);
      expect(store.list.totalItems).toBe(1);
      expect(store.list.totalPages).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle load items failure', async () => {
      const errorMessage = 'Failed to load members.';
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
      mockLoadItems.mockResolvedValue(ok(mockPaginatedMembers)); // _loadItems is called after successful add
      const result = await store.addItem({ ...mockMember });

      expect(result.ok).toBe(true);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle add item failure', async () => {
      const errorMessage = 'Failed to add member.';
      mockAdd.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.addItem({ ...mockMember });

      expect(result.ok).toBe(false);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('updateItem', () => {
    it('should update an item successfully', async () => {
      mockLoadItems.mockResolvedValue(ok(mockPaginatedMembers));

      await store.updateItem(mockMember);

      expect(store.update.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle update item failure', async () => {
      const errorMessage = 'Failed to update member.';
      mockUpdate.mockResolvedValue(err({ message: errorMessage } as ApiError));

      await store.updateItem(mockMember);

      expect(store.update.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('deleteItem', () => {
    it('should delete an item successfully', async () => {
      mockLoadItems.mockResolvedValue(ok(mockPaginatedMembers));

      const result = await store.deleteItem(mockMember.id!);

      expect(result.ok).toBe(true);
      expect(store._delete.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle delete item failure', async () => {
      const errorMessage = 'Failed to delete member.';
      mockDelete.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.deleteItem(mockMember.id!);

      expect(result.ok).toBe(false);
      expect(store._delete.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });

  describe('getById', () => {
    it('should get an item by ID successfully', async () => {
      mockGetById.mockResolvedValue(ok(mockMember));

      const result = await store.getById(mockMember.id!);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toEqual(mockMember);
      expect(result).toEqual(mockMember);
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });

    it('should handle get by ID failure', async () => {
      const errorMessage = 'Failed to load member.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getById(mockMember.id!);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });
  });

  describe('getByIds', () => {
    it('should get items by IDs successfully', async () => {
      mockGetByIds.mockResolvedValue(ok([mockMember]));

      const result = await store.getByIds([mockMember.id!]);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockMember]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });

    it('should handle get by IDs failure', async () => {
      const errorMessage = 'Failed to load members.';
      mockGetByIds.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getByIds([mockMember.id!]);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(result).toEqual([]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItems', () => {
    it('should add multiple items successfully', async () => {
      mockAddItems.mockResolvedValue(ok(['new-id-1', 'new-id-2']));
      mockLoadItems.mockResolvedValue(ok(mockPaginatedMembers));

      const newMembers = [
        { familyId: 'f1', lastName: 'Doe', firstName: 'John', gender: Gender.Male },
        { familyId: 'f1', lastName: 'Doe', firstName: 'Jane', gender: Gender.Female },
      ];
      const result = await store.addItems(newMembers);

      expect(result.ok).toBe(true);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAddItems).toHaveBeenCalledWith(newMembers);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle add multiple items failure', async () => {
      const errorMessage = 'Failed to add multiple members.';
      mockAddItems.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const newMembers = [
        { familyId: 'f1', lastName: 'Doe', firstName: 'John', gender: Gender.Male },
      ];
      const result = await store.addItems(newMembers);

      expect(result.ok).toBe(false);
      expect(store.add.loading).toBe(false);
      expect(store.error).toBeTruthy();
      expect(mockAddItems).toHaveBeenCalledWith(newMembers);
      expect(mockLoadItems).not.toHaveBeenCalled();
    });
  });
});