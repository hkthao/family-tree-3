import { setActivePinia, createPinia } from 'pinia';
import { useUserProfileStore } from '@/stores/user-profile.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ok, err } from '@/types';
import type { UserProfile } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the ICurrentUserProfileService
const mockGetCurrentUserProfile = vi.fn();
const mockUpdateUserProfile = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    userProfile: {
      getCurrentUserProfile: mockGetCurrentUserProfile,
      updateUserProfile: mockUpdateUserProfile,
    },
    // Add other services as empty objects if they are not directly used by user-profile.store
    ai: {},
    auth: {},
    chat: {},
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
    userActivity: {},
    userPreference: {},
    userSettings: {},
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

describe('user-profile.store', () => {
  let store: ReturnType<typeof useUserProfileStore>;

  const mockUserProfile: UserProfile = {
    id: 'user1',
    externalId: 'auth0|123',
    email: 'test@example.com',
    name: 'Test User',
    avatar: 'http://example.com/avatar.jpg',
    userId: ''
  };

  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useUserProfileStore();
    store.$reset();
    // Inject the mocked services
    store.services = createServices('test');

    // Reset mocks before each test
    mockGetCurrentUserProfile.mockReset();
    mockUpdateUserProfile.mockReset();

    // Set default mock resolved values
    mockGetCurrentUserProfile.mockResolvedValue(ok(mockUserProfile));
    mockUpdateUserProfile.mockResolvedValue(ok(mockUserProfile));
  });

  it('should have correct initial state', () => {
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.userProfile).toBeNull();
  });

  describe('fetchCurrentUserProfile', () => {
    // Mục tiêu của test: Đảm bảo rằng action fetchCurrentUserProfile xử lý thành công việc lấy profile của người dùng hiện tại.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getCurrentUserProfile trả về thành công.
    it('should fetch current user profile successfully', async () => {
      // Act: Gọi action fetchCurrentUserProfile.
      await store.fetchCurrentUserProfile();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.userProfile).toEqual(mockUserProfile);
      expect(mockGetCurrentUserProfile).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - userProfile phải được cập nhật với dữ liệu trả về từ service.
      // - Service getCurrentUserProfile phải được gọi chính xác một lần.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchCurrentUserProfile xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getCurrentUserProfile trả về lỗi.
    it('should handle fetch current user profile failure from service', async () => {
      const errorMessage = 'Failed to fetch current profile.';
      mockGetCurrentUserProfile.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action fetchCurrentUserProfile.
      await store.fetchCurrentUserProfile();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.profile.fetchError'); // i18n mock returns the key
      expect(store.userProfile).toBeNull();
      expect(mockGetCurrentUserProfile).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - userProfile không được thay đổi.
      // - Service getCurrentUserProfile phải được gọi chính xác một lần.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchCurrentUserProfile xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getCurrentUserProfile ném ra lỗi.
    it('should handle unexpected error during fetch current user profile', async () => {
      const errorMessage = 'Network error.';
      mockGetCurrentUserProfile.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action fetchCurrentUserProfile.
      await store.fetchCurrentUserProfile();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.profile.unexpectedError'); // i18n mock returns the key
      expect(store.userProfile).toBeNull();
      expect(mockGetCurrentUserProfile).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - userProfile không được thay đổi.
      // - Service getCurrentUserProfile phải được gọi chính xác một lần.
    });
  });

  describe('updateUserProfile', () => {
    // Mục tiêu của test: Đảm bảo rằng action updateUserProfile xử lý thành công việc cập nhật profile người dùng.

    // Các bước (Arrange, Act, Assert):
    it('should update user profile successfully', async () => {
      const updatedProfile: UserProfile = { ...mockUserProfile, name: 'Updated User' };
      store.userProfile = mockUserProfile; // Set initial profile
      mockUpdateUserProfile.mockResolvedValue(ok(updatedProfile));

      // Act: Gọi action updateUserProfile.
      const result = await store.updateUserProfile(updatedProfile);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.userProfile).toEqual(updatedProfile);
      expect(mockUpdateUserProfile).toHaveBeenCalledTimes(1);
      expect(mockUpdateUserProfile).toHaveBeenCalledWith(updatedProfile);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về true khi cập nhật thành công.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - userProfile phải được cập nhật với dữ liệu trả về từ service.
      // - Service updateUserProfile phải được gọi chính xác một lần với profile đã cho.
    });

    // Mục tiêu của test: Đảm bảo rằng action updateUserProfile xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service updateUserProfile trả về lỗi.
    it('should handle update user profile failure from service', async () => {
      const updatedProfile: UserProfile = { ...mockUserProfile, name: 'Updated User' };
      store.userProfile = mockUserProfile; // Set initial profile
      const errorMessage = 'Failed to update profile.';
      mockUpdateUserProfile.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action updateUserProfile.
      const result = await store.updateUserProfile(updatedProfile);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.profile.saveError'); // i18n mock returns the key
      expect(store.userProfile).toEqual(mockUserProfile); // Should not update on failure
      expect(mockUpdateUserProfile).toHaveBeenCalledTimes(1);
      expect(mockUpdateUserProfile).toHaveBeenCalledWith(updatedProfile);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về false khi cập nhật thất bại.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - userProfile không được thay đổi.
      // - Service updateUserProfile phải được gọi chính xác một lần với profile đã cho.
    });

    // Mục tiêu của test: Đảm bảo rằng action updateUserProfile xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service updateUserProfile ném ra lỗi.
    it('should handle unexpected error during update user profile', async () => {
      const updatedProfile: UserProfile = { ...mockUserProfile, name: 'Updated User' };
      store.userProfile = mockUserProfile; // Set initial profile
      const errorMessage = 'Network error.';
      mockUpdateUserProfile.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action updateUserProfile.
      const result = await store.updateUserProfile(updatedProfile);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.profile.unexpectedError'); // i18n mock returns the key
      expect(store.userProfile).toEqual(mockUserProfile); // Should not update on failure
      expect(mockUpdateUserProfile).toHaveBeenCalledTimes(1);
      expect(mockUpdateUserProfile).toHaveBeenCalledWith(updatedProfile);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về false khi có lỗi không mong muốn.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - userProfile không được thay đổi.
      // - Service updateUserProfile phải được gọi chính xác một lần với profile đã cho.
    });
  });

  describe('reset', () => {
    // Mục tiêu của test: Đảm bảo rằng action reset đặt lại tất cả các trạng thái về giá trị ban đầu.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Thay đổi trạng thái của store để không phải là trạng thái ban đầu.
    it('should reset the store state', () => {
      store.loading = true;
      store.error = 'Some error';
      store.userProfile = mockUserProfile;

      // Act: Gọi action reset.
      store.reset();

      // Assert: Kiểm tra rằng tất cả các trạng thái đã được đặt lại.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.userProfile).toBeNull();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - loading, error, userProfile và allUserProfiles phải trở về giá trị mặc định của chúng.
    });
  });
});