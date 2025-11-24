import { setActivePinia, createPinia } from 'pinia';
import { useFamilyLookupStore } from '@/stores/familyLookup.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Family } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';
import i18n from '@/plugins/i18n';

// Mock the IFamilyService
const mockGetByIds = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    family: {
      getByIds: mockGetByIds,
      // Add other family service methods as empty objects if not directly used
      fetch: vi.fn(),
      getById: vi.fn(),
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
    },
    // Add other services as empty objects
    ai: {}, auth: {}, chat: {}, event: {}, face: {},
    member: {}, naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {},
    userSettings: {}, familyDict: {}, familyData: {}, memberLookup: {}, eventCalendar: {}, eventTimeline: {},
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key), // Mock the translation function to return the key itself
    },
  },
}));

describe('familyLookup.store', () => {
  let store: ReturnType<typeof useFamilyLookupStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFamilyLookupStore();
    store.$reset();
    store.services = createServices('test');
    mockGetByIds.mockReset();

    // Clear cache before each test
    store.clearCache();
  });

  const mockFamily1: Family = {
    id: 'family-1',
    name: 'Family One',
    description: 'Desc One',
    address: 'Address One',
    avatarUrl: 'url-1',
    visibility: 0,
    familyUsers: [],
  };

  const mockFamily2: Family = {
    id: 'family-2',
    name: 'Family Two',
    description: 'Desc Two',
    address: 'Address Two',
    avatarUrl: 'url-2',
    visibility: 0,
    familyUsers: [],
  };

  describe('getByIds', () => {
    it('should fetch families from API and cache them', async () => {
      mockGetByIds.mockResolvedValue(ok([mockFamily1, mockFamily2]));

      const result = await store.getByIds([mockFamily1.id, mockFamily2.id]);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockFamily1, mockFamily2]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
      expect(mockGetByIds).toHaveBeenCalledWith([mockFamily1.id, mockFamily2.id]);
      expect(store.familyCache.get(mockFamily1.id)).toEqual(mockFamily1);
      expect(store.familyCache.get(mockFamily2.id)).toEqual(mockFamily2);
    });

    it('should return cached families if available', async () => {
      store.cacheFamily(mockFamily1); // Pre-cache one family
      mockGetByIds.mockResolvedValue(ok([mockFamily2])); // Only API call for family2

      const result = await store.getByIds([mockFamily1.id, mockFamily2.id]);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockFamily1, mockFamily2]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
      expect(mockGetByIds).toHaveBeenCalledWith([mockFamily2.id]); // Only family2 was fetched
      expect(store.familyCache.get(mockFamily1.id)).toEqual(mockFamily1);
      expect(store.familyCache.get(mockFamily2.id)).toEqual(mockFamily2);
    });

    it('should handle API call failure', async () => {
      const errorMessage = 'Failed to load families by IDs.';
      mockGetByIds.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getByIds([mockFamily1.id]);

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage); // Expect the error message directly
      expect(result).toEqual([]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetByIds.mockResolvedValue(ok([mockFamily1]));
      const promise = store.getByIds([mockFamily1.id]);

      expect(store.loading).toBe(true);
      await promise;
      expect(store.loading).toBe(false);
    });
  });

  describe('cacheFamily', () => {
    it('should add a family to the cache', () => {
      store.cacheFamily(mockFamily1);
      expect(store.familyCache.get(mockFamily1.id)).toEqual(mockFamily1);
    });

    it('should update an existing family in the cache', () => {
      store.cacheFamily(mockFamily1);
      const updatedFamily = { ...mockFamily1, name: 'Updated Name' };
      store.cacheFamily(updatedFamily);
      expect(store.familyCache.get(mockFamily1.id)).toEqual(updatedFamily);
    });
  });

  describe('clearCache', () => {
    it('should clear all entries from the cache', () => {
      store.cacheFamily(mockFamily1);
      store.cacheFamily(mockFamily2);
      expect(store.familyCache.size).toBe(2);

      store.clearCache();

      expect(store.familyCache.size).toBe(0);
    });
  });
});
