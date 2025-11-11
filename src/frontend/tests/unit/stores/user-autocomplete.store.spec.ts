import { setActivePinia, createPinia } from 'pinia';
import { useUserAutocompleteStore } from '@/stores/user-autocomplete.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { User } from '@/types';
import { Result, ok, err, ApiError } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key: string) => key), // Mock translation function
    },
  },
}));

// Mock services
const mockSearch = vi.fn();
const mockGetByIds = vi.fn();

vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    user: {
      search: mockSearch,
      getByIds: mockGetByIds,
    },
  })),
}));

describe('user-autocomplete.store', () => {
  let store: ReturnType<typeof useUserAutocompleteStore>;

  const mockUser1: User = {
    id: 'user-1',
    email: 'test1@example.com',
    roles: ['User'],
    externalId: '',
    name: 'Test User 1',
  };
  const mockUser2: User = {
    id: 'user-2',
    email: 'test2@example.com',
    roles: ['User'],
    externalId: '',
    name: 'Test User 2',
  };

  beforeEach(() => {
    setActivePinia(createPinia());
    store = useUserAutocompleteStore();
    store.$reset();
    store.services = createServices('test');

    mockSearch.mockReset();
    mockGetByIds.mockReset();

    // Default successful mocks
    mockSearch.mockResolvedValue(ok({ items: [mockUser1], totalCount: 1 }));
    mockGetByIds.mockResolvedValue(ok([mockUser1, mockUser2]));

    // Mock console.error to prevent it from polluting test output
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  it('should have correct initial state', () => {
    expect(store.items).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('actions', () => {
    describe('search', () => {
      it('should search for users successfully', async () => {
        const filters = { searchQuery: 'test' };
        const result = await store.search(filters);

        expect(mockSearch).toHaveBeenCalledWith(filters.searchQuery, 1, 50);
        expect(store.items).toEqual([mockUser1]);
        expect(store.loading).toBe(false);
        expect(store.error).toBeNull();
        expect(result).toEqual([mockUser1]);
      });

      it('should handle search failure', async () => {
        const testError: ApiError = { name: 'ApiError', message: 'Search failed' };
        mockSearch.mockResolvedValue(err(testError));
        const filters = { searchQuery: 'test' };

        const result = await store.search(filters);

        expect(mockSearch).toHaveBeenCalledWith(filters.searchQuery, 1, 50);
        expect(store.items).toEqual([]);
        expect(store.loading).toBe(false);
        expect(store.error).toBe('common.errors.loadUsers'); // From i18n mock
        expect(result).toEqual([]);
        expect(console.error).toHaveBeenCalledWith(testError);
      });

      it('should search with empty query if not provided', async () => {
        const filters = {};
        await store.search(filters);
        expect(mockSearch).toHaveBeenCalledWith('', 1, 50);
      });
    });

    describe('getByIds', () => {
      it('should get users by IDs successfully', async () => {
        const ids = ['user-1', 'user-2'];
        const result = await store.getByIds(ids);

        expect(mockGetByIds).toHaveBeenCalledWith(ids);
        expect(result).toEqual([mockUser1, mockUser2]);
      });

      it('should handle getByIds failure', async () => {
        const testError: ApiError = { name: 'ApiError', message: 'Get by IDs failed' };
        mockGetByIds.mockResolvedValue(err(testError));
        const ids = ['user-1'];

        const result = await store.getByIds(ids);

        expect(mockGetByIds).toHaveBeenCalledWith(ids);
        expect(result).toEqual([]);
        expect(console.error).toHaveBeenCalledWith(testError);
      });
    });

    describe('clearItems', () => {
      it('should clear the items array', () => {
        store.items = [mockUser1, mockUser2];
        store.clearItems();
        expect(store.items).toEqual([]);
      });
    });
  });
});
