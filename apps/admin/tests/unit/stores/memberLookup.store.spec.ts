import { setActivePinia, createPinia } from 'pinia';
import { useMemberLookupStore } from '@/stores/memberLookup.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, Member } from '@/types';
import { ok, err, Gender } from '@/types';
import { createServices } from '@/services/service.factory';
// import i18n from '@/plugins/i18n'; // REMOVED

// Mock the IMemberService
const mockGetByIds = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    member: {
      getByIds: mockGetByIds,
      // Add other member service methods as empty objects if not directly used
      fetch: vi.fn(),
      getById: vi.fn(),
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
    },
    // Add other services as empty objects
    ai: {}, auth: {}, chat: {}, event: {}, face: {}, family: {},
    naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {},
    userSettings: {}, familyDict: {}, familyData: {}, familyLookup: {}, eventCalendar: {}, eventTimeline: {},
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

describe('memberLookup.store', () => {
  let store: ReturnType<typeof useMemberLookupStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useMemberLookupStore();
    store.$reset();
    store.services = createServices('test');
    mockGetByIds.mockReset();

    // Clear cache before each test
    store.clearCache();
  });

  const mockMember1: Member = {
    id: 'member-1',
    lastName: 'Nguyen',
    firstName: 'Van A',
    fullName: 'Nguyen Van A',
    gender: Gender.Male,
    familyId: 'family-1',
    dateOfBirth: new Date('1990-01-01'),
  };

  const mockMember2: Member = {
    id: 'member-2',
    lastName: 'Tran',
    firstName: 'Thi B',
    fullName: 'Tran Thi B',
    gender: Gender.Female,
    familyId: 'family-1',
    dateOfBirth: new Date('1995-05-05'),
  };

  describe('getByIds', () => {
    it('should fetch members from API and cache them', async () => {
      mockGetByIds.mockResolvedValue(ok([mockMember1, mockMember2]));

      const result = await store.getByIds([mockMember1.id, mockMember2.id]);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockMember1, mockMember2]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
      expect(mockGetByIds).toHaveBeenCalledWith([mockMember1.id, mockMember2.id]);
      expect(store.memberCache.get(mockMember1.id)).toEqual(mockMember1);
      expect(store.memberCache.get(mockMember2.id)).toEqual(mockMember2);
    });

    it('should return cached members if available', async () => {
      store.cacheMember(mockMember1); // Pre-cache one member
      mockGetByIds.mockResolvedValue(ok([mockMember2])); // Only API call for member2

      const result = await store.getByIds([mockMember1.id, mockMember2.id]);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockMember1, mockMember2]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
      expect(mockGetByIds).toHaveBeenCalledWith([mockMember2.id]); // Only member2 was fetched
      expect(store.memberCache.get(mockMember1.id)).toEqual(mockMember1);
      expect(store.memberCache.get(mockMember2.id)).toEqual(mockMember2);
    });

    it('should handle API call failure', async () => {
      const errorMessage = 'Failed to load members by IDs.';
      mockGetByIds.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getByIds([mockMember1.id]);

      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage); // Expect the error message directly
      expect(result).toEqual([]);
      expect(mockGetByIds).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetByIds.mockResolvedValue(ok([mockMember1]));
      const promise = store.getByIds([mockMember1.id]);

      expect(store.loading).toBe(true);
      await promise;
      expect(store.loading).toBe(false);
    });
  });

  describe('cacheMember', () => {
    it('should add a member to the cache', () => {
      store.cacheMember(mockMember1);
      expect(store.memberCache.get(mockMember1.id)).toEqual(mockMember1);
    });

    it('should update an existing member in the cache', () => {
      store.cacheMember(mockMember1);
      const updatedMember = { ...mockMember1, fullName: 'Updated Name' };
      store.cacheMember(updatedMember);
      expect(store.memberCache.get(mockMember1.id)).toEqual(updatedMember);
    });
  });

  describe('clearCache', () => {
    it('should clear all entries from the cache', () => {
      store.cacheMember(mockMember1);
      store.cacheMember(mockMember2);
      expect(store.memberCache.size).toBe(2);

      store.clearCache();

      expect(store.memberCache.size).toBe(0);
    });
  });
});
