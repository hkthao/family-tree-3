import { setActivePinia, createPinia } from 'pinia';
import { usePublicRelationshipStore } from '@/stores/publicRelationship.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Relationship } from '@/types';
import { ok, err, RelationshipType } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IPublicRelationshipService
const mockGetPublicRelationshipsByFamilyId = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    publicRelationship: {
      getPublicRelationshipsByFamilyId: mockGetPublicRelationshipsByFamilyId,
      // Add other publicRelationship service methods as empty objects if not directly used
    },
    // Add other services as empty objects
    ai: {}, auth: {}, chat: {}, event: {}, face: {}, family: {},
    member: {}, naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {},
    userSettings: {}, familyDict: {}, familyData: {}, familyLookup: {}, eventCalendar: {}, eventTimeline: {}, memberLookup: {}, nlEditor: {}, privacyConfiguration: {}, publicFamily: {}, publicMember: {},
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

describe('publicRelationship.store', () => {
  let store: ReturnType<typeof usePublicRelationshipStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = usePublicRelationshipStore();
    store.$reset();
    store.services = createServices('test');
    mockGetPublicRelationshipsByFamilyId.mockReset();

    // Default mock resolved values
    mockGetPublicRelationshipsByFamilyId.mockResolvedValue(ok(mockRelationships));
  });

  const mockRelationships: Relationship[] = [
    {
      id: 'rel-1',
      sourceMemberId: 'member-1',
      targetMemberId: 'member-2',
      type: RelationshipType.Child, // Fixed: Used a valid RelationshipType
      familyId: 'family-1',
    },
    {
      id: 'rel-2',
      sourceMemberId: 'member-3',
      targetMemberId: 'member-4',
      type: RelationshipType.Husband, // Fixed: Used a valid RelationshipType
      familyId: 'family-1',
    },
  ];

  describe('getPublicRelationshipsByFamilyId', () => {
    it('should load public relationships successfully', async () => {
      mockGetPublicRelationshipsByFamilyId.mockResolvedValue(ok(mockRelationships));

      const result = await store.getPublicRelationshipsByFamilyId('family-1');

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.list.items).toEqual(mockRelationships);
      expect(result).toEqual(mockRelationships);
      expect(mockGetPublicRelationshipsByFamilyId).toHaveBeenCalledTimes(1);
      expect(mockGetPublicRelationshipsByFamilyId).toHaveBeenCalledWith('family-1');
    });

    it('should handle load public relationships failure', async () => {
      const errorMessage = 'Failed to load public relationships.';
      mockGetPublicRelationshipsByFamilyId.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getPublicRelationshipsByFamilyId('family-1');

      expect(store.list.loading).toBe(false);
      expect(store.error).toBe('relationship.errors.load');
      expect(store.list.items).toEqual([]);
      expect(result).toEqual([]);
      expect(mockGetPublicRelationshipsByFamilyId).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetPublicRelationshipsByFamilyId.mockResolvedValue(ok(mockRelationships));
      const promise = store.getPublicRelationshipsByFamilyId('family-1');

      expect(store.list.loading).toBe(true);
      await promise;
      expect(store.list.loading).toBe(false);
    });
  });
});
