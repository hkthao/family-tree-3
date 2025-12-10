import { setActivePinia, createPinia } from 'pinia';
import { useUserActivityStore } from '@/stores/user-activity.store';
import { ok, err } from '@/types';
import type { ApiError, RecentActivity } from '@/types';
import { TargetType, UserActionType } from '@/types';
import { createServices } from '@/services/service.factory';
import { beforeEach, describe, expect, it, vi } from 'vitest';
const mockGetRecentActivities = vi.fn();
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    user: {
      getRecentActivities: mockGetRecentActivities,
    },
  })),
}));
describe('user-activity.store', () => {
  let store: ReturnType<typeof useUserActivityStore>;
  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useUserActivityStore();
    store.$reset();
    store.services = createServices('test');
    mockGetRecentActivities.mockReset();
  });
  it('should have correct initial state', () => {
    expect(store.items).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });
  describe('fetchRecentActivities', () => {
    it('should fetch recent activities successfully', async () => {
      const mockActivities: RecentActivity[] = [
        {
          id: '1',
          userProfileId: 'user1',
          actionType: UserActionType.CreateEvent,
          targetType: TargetType.Family,
          targetId: 'fam1',
          activitySummary: 'User added a family',
          created: '2023-01-01T10:00:00Z',
        },
        {
          id: '2',
          userProfileId: 'user1',
          actionType: UserActionType.CreateEvent,
          targetType: TargetType.Member,
          targetId: 'mem1',
          activitySummary: 'User updated a member',
          created: '2023-01-01T11:00:00Z',
        },
      ];
      mockGetRecentActivities.mockResolvedValue(ok({ items: mockActivities, page: 1, totalPages: 1, totalItems: 2 }));
      await store.fetchRecentActivities();
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.items).toEqual(mockActivities);
      expect(mockGetRecentActivities).toHaveBeenCalledTimes(1);
      expect(mockGetRecentActivities).toHaveBeenCalledWith(
        1,
        10,
        undefined,
        undefined,
        undefined,
      );
    });
    it('should handle fetch recent activities failure from service', async () => {
      const errorMessage = 'Failed to fetch recent activities.';
      mockGetRecentActivities.mockResolvedValue(
        err({ message: errorMessage } as ApiError),
      );
      await store.fetchRecentActivities();
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.items).toEqual([]);
      expect(mockGetRecentActivities).toHaveBeenCalledTimes(1);
    });
    it('should handle unexpected error during fetch recent activities', async () => {
      const errorMessage = 'Network error.';
      mockGetRecentActivities.mockRejectedValue(new Error(errorMessage));
      await store.fetchRecentActivities();
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.items).toEqual([]);
      expect(mockGetRecentActivities).toHaveBeenCalledTimes(1);
    });
    it('should call service with provided parameters', async () => {
      const limit = 5;
      const targetType: TargetType = TargetType.Family;
      const targetId = 'fam123';
      const groupId = 'group456';
      const page = 1;
      mockGetRecentActivities.mockResolvedValue(ok({ items: [], page: 1, totalPages: 1, totalItems: 0 }));
      await store.fetchRecentActivities(limit, targetType, targetId, groupId, page);
      expect(mockGetRecentActivities).toHaveBeenCalledWith(
        page,
        limit,
        targetType,
        targetId,
        groupId,
      );
    });
  });
});
