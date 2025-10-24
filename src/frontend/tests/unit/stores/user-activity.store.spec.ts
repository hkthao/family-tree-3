import { setActivePinia, createPinia } from 'pinia';
import { useUserActivityStore } from '@/stores/user-activity.store';
import { ok, err } from '@/types';
import type { RecentActivity } from '@/types';
import { TargetType, UserActionType } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';
import { beforeEach, describe, expect, it, vi } from 'vitest';

// Mock the IUserActivityService
const mockGetRecentActivities = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    userActivity: {
      getRecentActivities: mockGetRecentActivities,
    },
    // Add other services as empty objects if they are not directly used by userActivity.store
    ai: {},
    auth: {},
    chat: {},
    chunk: {},
    dashboard: {},
    event: {},
    face: {},
    family: {},
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('userActivity.store', () => {
  let store: ReturnType<typeof useUserActivityStore>;

  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useUserActivityStore();
    store.$reset();
    // Inject the mocked services
    store.services = createServices('mock');

    // Reset mocks before each test
    mockGetRecentActivities.mockReset();
  });

  it('should have correct initial state', () => {
    expect(store.items).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('fetchRecentActivities', () => {
    // Mục tiêu của test: Đảm bảo rằng action fetchRecentActivities xử lý thành công việc lấy các hoạt động gần đây.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getRecentActivities trả về thành công.
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
      mockGetRecentActivities.mockResolvedValue(ok(mockActivities));

      // Act: Gọi action fetchRecentActivities.
      await store.fetchRecentActivities();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.items).toEqual(mockActivities);
      expect(mockGetRecentActivities).toHaveBeenCalledTimes(1);
      expect(mockGetRecentActivities).toHaveBeenCalledWith(
        undefined,
        undefined,
        undefined,
        undefined,
      );
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - items phải được cập nhật với dữ liệu trả về từ service.
      // - Service getRecentActivities phải được gọi chính xác một lần với các tham số mặc định.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchRecentActivities xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getRecentActivities trả về lỗi.
    it('should handle fetch recent activities failure from service', async () => {
      const errorMessage = 'Failed to fetch recent activities.';
      mockGetRecentActivities.mockResolvedValue(
        err({ message: errorMessage } as ApiError),
      );

      // Act: Gọi action fetchRecentActivities.
      await store.fetchRecentActivities();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.items).toEqual([]);
      expect(mockGetRecentActivities).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - items không được thay đổi.
      // - Service getRecentActivities phải được gọi chính xác một lần.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchRecentActivities xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getRecentActivities ném ra lỗi.
    it('should handle unexpected error during fetch recent activities', async () => {
      const errorMessage = 'Network error.';
      mockGetRecentActivities.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action fetchRecentActivities.
      await store.fetchRecentActivities();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.items).toEqual([]);
      expect(mockGetRecentActivities).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - items không được thay đổi.
      // - Service getRecentActivities phải được gọi chính xác một lần.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchRecentActivities gọi service với các tham số được cung cấp.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getRecentActivities trả về thành công.
    it('should call service with provided parameters', async () => {
      const limit = 5;
      const targetType: TargetType = TargetType.Family;
      const targetId = 'fam123';
      const groupId = 'group456';
      mockGetRecentActivities.mockResolvedValue(ok([]));

      // Act: Gọi action fetchRecentActivities với các tham số.
      await store.fetchRecentActivities(limit, targetType, targetId, groupId);

      // Assert: Kiểm tra rằng service được gọi với các tham số chính xác.
      expect(mockGetRecentActivities).toHaveBeenCalledWith(
        limit,
        targetType,
        targetId,
        groupId,
      );
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Service getRecentActivities phải được gọi với tất cả các tham số đã truyền vào.
    });
  });
});
