import { setActivePinia, createPinia } from 'pinia';
import { useFamilyAutocompleteStore } from '@/stores/family-autocomplete.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import i18n from '@/plugins/i18n';
import type { Family } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IFamilyService methods used by family-autocomplete.store
const mockLoadItems = vi.fn();
const mockGetByIds = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    family: {
      loadItems: mockLoadItems,
      getByIds: mockGetByIds,
      // Add other services as empty objects if they are not directly used by family-autocomplete.store
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getById: vi.fn(),
      addItems: vi.fn(),
    },
    ai: {}, auth: {}, chat: {}, dashboard: {}, event: {}, face: {}, faceMember: {},
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

describe('family-autocomplete.store', () => {
  let store: ReturnType<typeof useFamilyAutocompleteStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFamilyAutocompleteStore();
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

  describe('searchFamilies', () => {
    it('should fetch families successfully', async () => {
      const mockFamilies: Family[] = [
        { id: '1', name: 'Family A' },
        { id: '2', name: 'Family B' },
      ];
      mockLoadItems.mockResolvedValue(ok({ items: mockFamilies, totalItems: 2, totalPages: 1 }));

      await store.search({ searchQuery: 'Family' });

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.items).toEqual(mockFamilies);
      expect(mockLoadItems).toHaveBeenCalledWith(
        { searchQuery: 'Family' },
        1,
        50,
      );
    });

    it('should handle fetch families failure', async () => {
      mockLoadItems.mockResolvedValue(err({} as ApiError)); // No message, so i18n.global.t will be called

      await store.search({ searchQuery: 'Family' });

      expect(store.loading).toBe(false);
      expect(store.error).toBe('family.errors.load');
      expect(store.items).toEqual([]);
      expect(i18n.global.t).toHaveBeenCalledWith('family.errors.load');
    });
  });

  describe('getByIds', () => {
    it('should fetch families by IDs successfully', async () => {
      const mockFamilies: Family[] = [
        { id: '1', name: 'Family A' },
        { id: '2', name: 'Family B' },
      ];
      mockGetByIds.mockResolvedValue(ok(mockFamilies));

      const fetchedFamilies = await store.getByIds(['1', '2']);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(fetchedFamilies).toEqual(mockFamilies);
      expect(mockGetByIds).toHaveBeenCalledWith(['1', '2']);
    });

    it('should handle fetch families by IDs failure', async () => {
      mockGetByIds.mockResolvedValue(err({} as ApiError)); // No message, so i18n.global.t will be called

      const fetchedFamilies = await store.getByIds(['1', '2']);

      expect(store.loading).toBe(false);
      expect(store.error).toBe('family.errors.loadById');
      expect(fetchedFamilies).toEqual([]);
      expect(i18n.global.t).toHaveBeenCalledWith('family.errors.loadById');
    });
  });
});
