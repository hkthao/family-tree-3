import { setActivePinia, createPinia } from 'pinia';
import { useNLEditorStore } from '@/stores/nlEditor.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Member } from '@/types';
import { ok, err, Gender } from '@/types';
import type { ApiError, Paginated } from '@/types';
import { createServices } from '@/services/service.factory';
import i18n from '@/plugins/i18n';

// Mock the IMemberService
const mockMemberLoadItems = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    member: {
      loadItems: mockMemberLoadItems,
      // Add other member service methods as empty objects if not directly used
      fetch: vi.fn(),
      getById: vi.fn(),
      add: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      getByIds: vi.fn(),
    },
    // Add other services as empty objects
    ai: {}, auth: {}, chat: {}, event: {}, face: {}, family: {},
    naturalLanguageInput: {}, notification: {}, relationship: {},
    systemConfig: {}, userActivity: {}, userPreference: {}, userProfile: {},
    userSettings: {}, familyDict: {}, familyData: {}, familyLookup: {}, eventCalendar: {}, eventTimeline: {}, memberLookup: {},
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

describe('nlEditor.store', () => {
  let store: ReturnType<typeof useNLEditorStore>;

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useNLEditorStore();
    store.$reset();
    store.services = createServices('test');
    mockMemberLoadItems.mockReset();

    // Default mock resolved values
    mockMemberLoadItems.mockResolvedValue(ok(mockPaginatedMembers));
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

  const mockPaginatedMembers: Paginated<Member> = {
    items: [mockMember],
    page: 1,
    totalItems: 1,
    totalPages: 1,
  };

  describe('searchMembers', () => {
    it('should load members successfully', async () => {
      mockMemberLoadItems.mockResolvedValue(ok(mockPaginatedMembers));
      const searchQuery = 'Nguyen';

      await store.searchMembers(searchQuery);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.list.items).toEqual([mockMember]);
      expect(mockMemberLoadItems).toHaveBeenCalledTimes(1);
      expect(mockMemberLoadItems).toHaveBeenCalledWith(
        { searchQuery: searchQuery },
        1,
        10,
      );
    });

    it('should handle load members failure', async () => {
      const errorMessage = 'Failed to load members.';
      mockMemberLoadItems.mockResolvedValue(err({ message: errorMessage } as ApiError));
      const searchQuery = 'Nguyen';

      await store.searchMembers(searchQuery);

      expect(store.list.loading).toBe(false);
      expect(store.error).toBe('member.errors.load');
      expect(store.list.items).toEqual([]);
      expect(mockMemberLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should set loading state correctly', async () => {
      mockMemberLoadItems.mockResolvedValue(ok(mockPaginatedMembers));
      const promise = store.searchMembers('Nguyen');

      expect(store.list.loading).toBe(true);
      await promise;
      expect(store.list.loading).toBe(false);
    });
  });
});
