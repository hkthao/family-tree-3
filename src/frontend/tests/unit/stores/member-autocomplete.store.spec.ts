import { setActivePinia, createPinia } from 'pinia';
import { useMemberAutocompleteStore } from '@/stores/member-autocomplete.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import i18n from '@/plugins/i18n';
import type { Member } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IMemberService methods used by member-autocomplete.store
const mockLoadItems = vi.fn();
const mockGetByIds = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    member: {
      loadItems: mockLoadItems,
      getByIds: mockGetByIds,
      // Add other services as empty objects if they are not directly used by member-autocomplete.store
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getById: vi.fn(),
      addItems: vi.fn(),
    },
    ai: {}, auth: {}, chat: {}, chunk: {}, dashboard: {}, event: {}, face: {}, faceMember: {},
    fileUpload: {}, family: {}, naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {}, userSettings: {},
  })),
}));

// Mock the i18n global object
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key), // Mock t function to return the key itself
    },
  },
}));

describe('member-autocomplete.store', () => {
  let store: ReturnType<typeof useMemberAutocompleteStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useMemberAutocompleteStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');
    // Reset mocks before each test
    mockLoadItems.mockReset();
    mockGetByIds.mockReset();
  });

  it('should have initial state values', () => {
    expect(store.items).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.items).toEqual([]);
  });

  describe('search', () => {
    it('should fetch members successfully', async () => {
      const mockMembers: Member[] = [
        { id: '1', fullName: 'Member A', lastName: 'A', firstName: 'Member', familyId: 'family-1' },
        { id: '2', fullName: 'Member B', lastName: 'B', firstName: 'Member', familyId: 'family-1' },
      ];
      mockLoadItems.mockResolvedValue(ok({ items: mockMembers, totalItems: 2, totalPages: 1 }));

      await store.search({ searchQuery: 'Member' });

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.items).toEqual(mockMembers);
      expect(mockLoadItems).toHaveBeenCalledWith(
        { searchQuery: 'Member' },
        1,
        50,
      );
    });

    it('should handle fetch members failure', async () => {
      mockLoadItems.mockResolvedValue(err({} as ApiError)); // No message, so i18n.global.t will be called

      await store.search({ searchQuery: 'Member' });

      expect(store.loading).toBe(false);
      expect(store.error).toBe('member.errors.load');
      expect(store.items).toEqual([]);
      expect(i18n.global.t).toHaveBeenCalledWith('member.errors.load');
    });
  });

  describe('getByIds', () => {
    it('should fetch members by IDs successfully', async () => {
      const mockMembers: Member[] = [
        { id: '1', fullName: 'Member A', lastName: 'A', firstName: 'Member', familyId: 'family-1' },
        { id: '2', fullName: 'Member B', lastName: 'B', firstName: 'Member', familyId: 'family-1' },
      ];
      mockGetByIds.mockResolvedValue(ok(mockMembers));

      const fetchedMembers = await store.getByIds(['1', '2']);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(fetchedMembers).toEqual(mockMembers);
      expect(mockGetByIds).toHaveBeenCalledWith(['1', '2']);
    });

    it('should handle fetch members by IDs failure', async () => {
      mockGetByIds.mockResolvedValue(err({} as ApiError)); // No message, so i18n.global.t will be called

      const fetchedMembers = await store.getByIds(['1', '2']);

      expect(store.loading).toBe(false);
      expect(store.error).toBe('member.errors.loadById');
      expect(fetchedMembers).toEqual([]);
      expect(i18n.global.t).toHaveBeenCalledWith('member.errors.loadById');
    });
  });
});