import { setActivePinia, createPinia } from 'pinia';
import { useFamilyAutocompleteStore } from '@/stores/familyAutocomplete.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import i18n from '@/plugins/i18n';
import type { Family } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IFamilyService methods used by familyAutocomplete.store
const mockLoadItems = vi.fn();
const mockGetByIds = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    family: {
      loadItems: mockLoadItems,
      getByIds: mockGetByIds,
      // Add other services as empty objects if they are not directly used by familyAutocomplete.store
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getById: vi.fn(),
      addItems: vi.fn(),
    },
    ai: {}, auth: {}, chat: {}, chunk: {}, dashboard: {}, event: {}, face: {}, faceMember: {},
    fileUpload: {}, member: {}, naturalLanguageInput: {}, notification: {}, relationship: {},
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

describe('familyAutocomplete.store', () => {
  let store: ReturnType<typeof useFamilyAutocompleteStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFamilyAutocompleteStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('mock');
    // Reset mocks before each test
    mockLoadItems.mockReset();
    mockGetByIds.mockReset();
  });

  const mockFamily: Family = {
    id: 'family-1',
    name: 'Test Family',
  };

  it('should have initial state values', () => {
    expect(store.families).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.items).toEqual([]);
  });

  describe('searchFamilies', () => {
    it('should fetch families successfully', async () => {
      const mockFamilies: Family[] = [
        { id: '1', name: 'Family A' },
        { id: '2', name: 'Family B' },
      ];
      mockLoadItems.mockResolvedValue(ok({ items: mockFamilies, totalItems: 2, totalPages: 1 }));

      await store.searchFamilies({ searchQuery: 'Family' });

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.families).toEqual(mockFamilies);
      expect(store.familyCache.get('1')).toEqual(mockFamilies[0]);
      expect(store.familyCache.get('2')).toEqual(mockFamilies[1]);
      expect(mockLoadItems).toHaveBeenCalledWith(
        { searchQuery: 'Family' },
        1,
        20,
      );
    });

    it('should handle fetch families failure', async () => {
      mockLoadItems.mockResolvedValue(err({} as ApiError)); // No message, so i18n.global.t will be called

      await store.searchFamilies({ searchQuery: 'Family' });

      expect(store.loading).toBe(false);
      expect(store.error).toBe('family.errors.load');
      expect(store.families).toEqual([]);
      expect(i18n.global.t).toHaveBeenCalledWith('family.errors.load');
    });
  });

  describe('getFamilyByIds', () => {
    it('should fetch families by IDs successfully', async () => {
      const mockFamilies: Family[] = [
        { id: '1', name: 'Family A' },
        { id: '2', name: 'Family B' },
      ];
      mockGetByIds.mockResolvedValue(ok(mockFamilies));

      const fetchedFamilies = await store.getFamilyByIds(['1', '2']);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(fetchedFamilies).toEqual(mockFamilies);
      expect(mockGetByIds).toHaveBeenCalledWith(['1', '2']);
    });

    it('should handle fetch families by IDs failure', async () => {
      mockGetByIds.mockResolvedValue(err({} as ApiError)); // No message, so i18n.global.t will be called

      const fetchedFamilies = await store.getFamilyByIds(['1', '2']);

      expect(store.loading).toBe(false);
      expect(store.error).toBe('family.errors.loadById');
      expect(fetchedFamilies).toEqual([]);
      expect(i18n.global.t).toHaveBeenCalledWith('family.errors.loadById');
    });

    it('should return cached families if available', async () => {
      const cachedFamily: Family = { id: '3', name: 'Family C' };
      store.familyCache.set(cachedFamily); // Correct usage of IdCache.set
      mockGetByIds.mockResolvedValue(ok([])); // Should not be called for cached item

      const fetchedFamilies = await store.getFamilyByIds(['3']);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(fetchedFamilies).toEqual([cachedFamily]);
      expect(mockGetByIds).not.toHaveBeenCalled();
    });
  });
});