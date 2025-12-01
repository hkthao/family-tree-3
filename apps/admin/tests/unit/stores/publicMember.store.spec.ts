import { setActivePinia, createPinia } from 'pinia';
import { usePublicMemberStore } from '@/stores/publicMember.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Member } from '@/types';
import { ok, err, Gender } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IPublicMemberService
const mockGetPublicMembersByFamilyId = vi.fn();
const mockGetPublicMemberById = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    publicMember: {
      getPublicMembersByFamilyId: mockGetPublicMembersByFamilyId,
      getPublicMemberById: mockGetPublicMemberById,
    },
    // Add other services as empty objects
    ai: {}, auth: {}, chat: {}, event: {}, face: {}, family: {},
    member: {}, naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {},
    userSettings: {}, familyDict: {}, familyData: {}, familyLookup: {}, eventCalendar: {}, eventTimeline: {}, memberLookup: {}, nlEditor: {}, privacyConfiguration: {}, publicFamily: {},
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

describe('publicMember.store', () => {
  let store: ReturnType<typeof usePublicMemberStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = usePublicMemberStore();
    store.$reset();
    store.services = createServices('test');
    mockGetPublicMembersByFamilyId.mockReset();
    mockGetPublicMemberById.mockReset();

    // Default mock resolved values
    mockGetPublicMembersByFamilyId.mockResolvedValue(ok([mockMember1, mockMember2]));
    mockGetPublicMemberById.mockResolvedValue(ok(mockMember1));
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

  describe('getPublicMembersByFamilyId', () => {
    it('should load public members successfully', async () => {
      mockGetPublicMembersByFamilyId.mockResolvedValue(ok([mockMember1, mockMember2]));

      const result = await store.getPublicMembersByFamilyId('family-1');

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(result).toEqual([mockMember1, mockMember2]);
      expect(mockGetPublicMembersByFamilyId).toHaveBeenCalledTimes(1);
      expect(mockGetPublicMembersByFamilyId).toHaveBeenCalledWith('family-1');
    });

    it('should handle load public members failure', async () => {
      const errorMessage = 'Failed to load public members.';
      mockGetPublicMembersByFamilyId.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getPublicMembersByFamilyId('family-1');

      expect(store.list.loading).toBe(false);
      expect(store.error).toBe('member.errors.load');
      expect(result).toEqual([]);
      expect(mockGetPublicMembersByFamilyId).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetPublicMembersByFamilyId.mockResolvedValue(ok([mockMember1]));
      const promise = store.getPublicMembersByFamilyId('family-1');

      expect(store.list.loading).toBe(true);
      await promise;
      expect(store.list.loading).toBe(false);
    });
  });

  describe('getPublicMemberById', () => {
    it('should load a specific public member successfully', async () => {
      mockGetPublicMemberById.mockResolvedValue(ok(mockMember1));

      const result = await store.getPublicMemberById(mockMember1.id, 'family-1');

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toEqual(mockMember1);
      expect(result).toEqual(mockMember1);
      expect(mockGetPublicMemberById).toHaveBeenCalledTimes(1);
      expect(mockGetPublicMemberById).toHaveBeenCalledWith(mockMember1.id, 'family-1');
    });

    it('should handle load public member failure', async () => {
      const errorMessage = 'Failed to load public member.';
      mockGetPublicMemberById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getPublicMemberById(mockMember1.id, 'family-1');

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBe('member.errors.loadById');
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetPublicMemberById).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetPublicMemberById.mockResolvedValue(ok(mockMember1));
      const promise = store.getPublicMemberById(mockMember1.id, 'family-1');

      expect(store.detail.loading).toBe(true);
      await promise;
      expect(store.detail.loading).toBe(false);
    });

    it('should return undefined if member is not found (result.value is null)', async () => {
      mockGetPublicMemberById.mockResolvedValue(ok(null));

      const result = await store.getPublicMemberById(mockMember1.id, 'family-1');

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetPublicMemberById).toHaveBeenCalledTimes(1);
    });
  });
});
