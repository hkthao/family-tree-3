import { setActivePinia, createPinia } from 'pinia';
import { useUserPreferenceStore } from '@/stores/user-preference.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ok, err } from '@/types';
import type { UserPreference } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { Theme, Language } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock the ICurrentUserPreferenceService
const mockGetUserPreferences = vi.fn();
const mockSaveUserPreferences = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    userPreference: {
      getUserPreferences: mockGetUserPreferences,
      saveUserPreferences: mockSaveUserPreferences,
    },
    // Add other services as empty objects if they are not directly used by userPreference.store
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
    userProfile: {},
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

describe('user-preference.store', () => {
  let store: ReturnType<typeof useUserPreferenceStore>;

  const initialPreferences: UserPreference = {
    id: '1',
    userProfileId: '1',
    theme: Theme.Light,
    language: Language.English,
    created: '2023-01-01T00:00:00Z',
    createdBy: 'system',
    lastModified: '2023-01-01T00:00:00Z',
    lastModifiedBy: 'system',
  };

  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useUserPreferenceStore();
    store.$reset();
    store.preferences = initialPreferences;
    // Inject the mocked services
    store.services = createServices('test');

    // Reset mocks before each test
    mockGetUserPreferences.mockReset();
    mockSaveUserPreferences.mockReset();

    // Set default mock resolved values
    mockGetUserPreferences.mockResolvedValue(ok(initialPreferences));
    mockSaveUserPreferences.mockResolvedValue(ok(undefined));
  });

  it('should have correct initial state', () => {
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.preferences).toEqual(initialPreferences);
  });

  describe('fetchUserPreferences', () => {
    // Mục tiêu của test: Đảm bảo rằng action fetchUserPreferences xử lý thành công việc lấy tùy chọn người dùng.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getUserPreferences trả về thành công.
    it('should fetch user preferences successfully', async () => {
      const fetchedPreferences: UserPreference = {
        id: '1',
        userProfileId: '1',
        theme: Theme.Dark,
        language: Language.Vietnamese,
        created: '2023-01-01T00:00:00Z',
        createdBy: 'system',
        lastModified: '2023-01-01T00:00:00Z',
        lastModifiedBy: 'system',
      };
      mockGetUserPreferences.mockResolvedValue(ok(fetchedPreferences));

      // Act: Gọi action fetchUserPreferences.
      await store.fetchUserPreferences();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.preferences).toEqual(fetchedPreferences);
      expect(mockGetUserPreferences).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - preferences phải được cập nhật với dữ liệu trả về từ service.
      // - Service getUserPreferences phải được gọi chính xác một lần.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchUserPreferences xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getUserPreferences trả về lỗi.
    it('should handle fetch user preferences failure from service', async () => {
      const errorMessage = 'Failed to fetch preferences.';
      mockGetUserPreferences.mockResolvedValue(
        err({ message: errorMessage } as ApiError),
      );

      // Act: Gọi action fetchUserPreferences.
      await store.fetchUserPreferences();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.preferences.fetchError'); // i18n mock returns the key
      expect(store.preferences).toEqual(initialPreferences); // Should revert to initial state or not change
      expect(mockGetUserPreferences).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - preferences không được thay đổi hoặc trở về trạng thái ban đầu.
      // - Service getUserPreferences phải được gọi chính xác một lần.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchUserPreferences xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getUserPreferences ném ra lỗi.
    it('should handle unexpected error during fetch user preferences', async () => {
      const errorMessage = 'Network error.';
      mockGetUserPreferences.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action fetchUserPreferences.
      await store.fetchUserPreferences();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.preferences.unexpectedError'); // i18n mock returns the key
      expect(store.preferences).toEqual(initialPreferences); // Should revert to initial state or not change
      expect(mockGetUserPreferences).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - preferences không được thay đổi hoặc trở về trạng thái ban đầu.
      // - Service getUserPreferences phải được gọi chính xác một lần.
    });
  });

  describe('saveUserPreferences', () => {
    // Mục tiêu của test: Đảm bảo rằng action saveUserPreferences xử lý thành công việc lưu tùy chọn người dùng.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, thay đổi preferences, mock service saveUserPreferences trả về thành công.
    it('should save user preferences successfully', async () => {
      const updatedPreferences: UserPreference = {
        id: '1',
        userProfileId: '1',
        theme: Theme.Dark,
        language: Language.Vietnamese,
        created: '2023-01-01T00:00:00Z',
        createdBy: 'system',
        lastModified: '2023-01-01T00:00:00Z',
        lastModifiedBy: 'system',
      };
      store.preferences = updatedPreferences; // Simulate user changing preferences
      mockSaveUserPreferences.mockResolvedValue(ok(undefined));

      // Act: Gọi action saveUserPreferences.
      await store.saveUserPreferences();

      // Assert: Kiểm tra trạng thái store và việc gọi service.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockSaveUserPreferences).toHaveBeenCalledTimes(1);
      expect(mockSaveUserPreferences).toHaveBeenCalledWith(updatedPreferences);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - Service saveUserPreferences phải được gọi chính xác một lần với các tùy chọn đã cập nhật.
    });

    // Mục tiêu của test: Đảm bảo rằng action saveUserPreferences xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, thay đổi preferences, mock service saveUserPreferences trả về lỗi.
    it('should handle save user preferences failure from service', async () => {
      const updatedPreferences: UserPreference = {
        id: '1',
        userProfileId: '1',
        theme: Theme.Dark,
        language: Language.Vietnamese,
        created: '2023-01-01T00:00:00Z',
        createdBy: 'system',
        lastModified: '2023-01-01T00:00:00Z',
        lastModifiedBy: 'system',
      };
      store.preferences = updatedPreferences; // Simulate user changing preferences
      const errorMessage = 'Failed to save preferences.';
      mockSaveUserPreferences.mockResolvedValue(
        err({ message: errorMessage } as ApiError),
      );

      // Act: Gọi action saveUserPreferences.
      await store.saveUserPreferences();

      // Assert: Kiểm tra trạng thái store và việc gọi service.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.preferences.saveError'); // i18n mock returns the key
      expect(mockSaveUserPreferences).toHaveBeenCalledTimes(1);
      expect(mockSaveUserPreferences).toHaveBeenCalledWith(updatedPreferences);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service saveUserPreferences phải được gọi chính xác một lần với các tùy chọn đã cập nhật.
    });

    // Mục tiêu của test: Đảm bảo rằng action saveUserPreferences xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    it('should handle unexpected error during save user preferences', async () => {
      const updatedPreferences: UserPreference = {
        id: '1',
        userProfileId: '1',
        theme: Theme.Dark,
        language: Language.Vietnamese,
        created: '2023-01-01T00:00:00Z',
        createdBy: 'system',
        lastModified: '2023-01-01T00:00:00Z',
        lastModifiedBy: 'system',
      };
      store.preferences = updatedPreferences; // Simulate user changing preferences
      const errorMessage = 'Network error.';
      mockSaveUserPreferences.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action saveUserPreferences.
      await store.saveUserPreferences();

      // Assert: Kiểm tra trạng thái store và việc gọi service.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('userSettings.preferences.unexpectedError'); // i18n mock returns the key
      expect(mockSaveUserPreferences).toHaveBeenCalledTimes(1);
      expect(mockSaveUserPreferences).toHaveBeenCalledWith(updatedPreferences);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service saveUserPreferences phải được gọi chính xác một lần với các tùy chọn đã cập nhật.
    });
  });
});
