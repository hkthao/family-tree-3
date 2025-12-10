import { setActivePinia, createPinia } from 'pinia';
import { usePublicFamilyStore } from '@/stores/publicFamily.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { type ApiError, type Family, FamilyVisibility } from '@/types'; 
import { ok, err } from '@/types';
import { createServices } from '@/services/service.factory';

const mockGetPublicFamilyById = vi.fn();

vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    publicFamily: {
      getPublicFamilyById: mockGetPublicFamilyById,
      
    },
    
    ai: {}, auth: {}, chat: {}, event: {}, face: {}, family: {},
    member: {}, naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {},
    userSettings: {}, familyDict: {}, familyData: {}, familyLookup: {}, eventCalendar: {}, eventTimeline: {}, memberLookup: {}, nlEditor: {}, privacyConfiguration: {},
  })),
}));


vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key), 
    },
  },
}));

describe('publicFamily.store', () => {
  let store: ReturnType<typeof usePublicFamilyStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = usePublicFamilyStore();
    store.$reset();
    store.services = createServices('test');
    mockGetPublicFamilyById.mockReset();

    
    mockGetPublicFamilyById.mockResolvedValue(ok(mockFamily));
  });

  const mockFamily: Family = {
    id: 'family-1',
    name: 'Test Public Family',
    description: 'A public family for testing',
    address: 'Public Address',
    avatarUrl: 'public-avatar.jpg',
    visibility: FamilyVisibility.Public, 
    familyUsers: [],
  };

  describe('getPublicFamilyById', () => {
    it('should load a public family successfully', async () => {
      mockGetPublicFamilyById.mockResolvedValue(ok(mockFamily));

      const result = await store.getPublicFamilyById(mockFamily.id);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toEqual(mockFamily);
      expect(result).toEqual(mockFamily);
      expect(mockGetPublicFamilyById).toHaveBeenCalledTimes(1);
      expect(mockGetPublicFamilyById).toHaveBeenCalledWith(mockFamily.id);
    });

    it('should handle load public family failure', async () => {
      const errorMessage = 'Failed to load public family.';
      mockGetPublicFamilyById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getPublicFamilyById(mockFamily.id);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBe('family.errors.loadById');
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetPublicFamilyById).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetPublicFamilyById.mockResolvedValue(ok(mockFamily));
      const promise = store.getPublicFamilyById(mockFamily.id);

      expect(store.detail.loading).toBe(true);
      await promise;
      expect(store.detail.loading).toBe(false);
    });

    it('should return undefined if family is not found (result.value is null)', async () => {
      mockGetPublicFamilyById.mockResolvedValue(ok(null));

      const result = await store.getPublicFamilyById(mockFamily.id);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetPublicFamilyById).toHaveBeenCalledTimes(1);
    });
  });
});
