import { setActivePinia, createPinia } from 'pinia';
import { useFaceMemberSelectStore } from '@/stores/faceMemberSelect.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Member } from '@/types';
import { ok, err, Gender } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';
import i18n from '@/plugins/i18n';

// Mock the IMemberService
const mockGetById = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    member: {
      getById: mockGetById,
      // Add other member service methods as empty objects if not directly used
      fetch: vi.fn(),
      loadItems: vi.fn(),
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getByIds: vi.fn(),
    },
    // Add other services as empty objects
    ai: {},
    auth: {},
    chat: {},
    event: {},
    face: {},
    family: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
    familyDict: {},
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key),
    },
  },
}));

describe('faceMemberSelect.store', () => {
  let store: ReturnType<typeof useFaceMemberSelectStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFaceMemberSelectStore();
    store.$reset();
    store.services = createServices('test');
    mockGetById.mockReset();

    // Default mock resolved values
    mockGetById.mockResolvedValue(ok(mockMember));
  });

  const mockMember: Member = {
    id: 'member-1',
    lastName: 'Nguyen',
    firstName: 'Van A',
    fullName: 'Nguyen Van A',
    gender: Gender.Male,
    familyId: 'family-1',
    dateOfBirth: new Date('1990-01-01'),
  };

  describe('getById', () => {
    it('should load a member successfully', async () => {
      mockGetById.mockResolvedValue(ok(mockMember));

      const result = await store.getById(mockMember.id);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.detail.item).toEqual(mockMember);
      expect(result).toEqual(mockMember);
      expect(mockGetById).toHaveBeenCalledTimes(1);
      expect(mockGetById).toHaveBeenCalledWith(mockMember.id);
    });

    it('should handle load member failure', async () => {
      const errorMessage = 'Failed to load member.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getById(mockMember.id);

      expect(store.detail.loading).toBe(false);
      expect(store.error).toBe('member.errors.loadById');
      expect(store.detail.item).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockGetById.mockResolvedValue(ok(mockMember));
      const promise = store.getById(mockMember.id);

      expect(store.detail.loading).toBe(true);
      await promise;
      expect(store.detail.loading).toBe(false);
    });
  });

  describe('setCurrentItem', () => {
    it('should set the current member item', () => {
      store.setCurrentItem(mockMember);
      expect(store.detail.item).toEqual(mockMember);
    });

    it('should set the current member item to null', () => {
      store.setCurrentItem(null);
      expect(store.detail.item).toBeNull();
    });
  });
});
