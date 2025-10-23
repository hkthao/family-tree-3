import { setActivePinia, createPinia } from 'pinia';
import { useSystemConfigStore } from '@/stores/system-config.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ok, err } from '@/types';
import type { SystemConfig } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the ISystemConfigService
const mockGetSystemConfigs = vi.fn();
const mockUpdateSystemConfig = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    systemConfig: {
      getSystemConfigs: mockGetSystemConfigs,
      updateSystemConfig: mockUpdateSystemConfig,
    },
    // Add other services as empty objects if they are not directly used by system-config.store
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
    userActivity: {},
    userPreference: {},
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

describe('systemConfig.store', () => {
  let store: ReturnType<typeof useSystemConfigStore>;

  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useSystemConfigStore();
    store.$reset();
    // Inject the mocked services
    store.services = createServices('mock');

    // Reset mocks before each test
    mockGetSystemConfigs.mockReset();
    mockUpdateSystemConfig.mockReset();

    // Mock console.error to prevent test output pollution
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  it('should have correct initial state', () => {
    expect(store.configs).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('fetchSystemConfigs', () => {
    // Mục tiêu của test: Đảm bảo rằng action fetchSystemConfigs xử lý thành công việc lấy cấu hình hệ thống.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getSystemConfigs trả về thành công.
    it('should fetch system configs successfully', async () => {
      const mockConfigs: SystemConfig[] = [
        { id: '1', key: 'key1', value: 'value1' },
        { id: '2', key: 'key2', value: 'value2' },
      ];
      mockGetSystemConfigs.mockResolvedValue(ok(mockConfigs));

      // Act: Gọi action fetchSystemConfigs.
      await store.fetchSystemConfigs();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.configs).toEqual(mockConfigs);
      expect(mockGetSystemConfigs).toHaveBeenCalledTimes(1);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - configs phải được cập nhật với dữ liệu trả về từ service.
      // - Service getSystemConfigs phải được gọi chính xác một lần.
    });

    // Mục tiêu của test: Đảm bảo rằng action fetchSystemConfigs xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service getSystemConfigs trả về lỗi.
    it('should handle fetch system configs failure', async () => {
      const errorMessage = 'Failed to fetch configs.';
      mockGetSystemConfigs.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action fetchSystemConfigs.
      await store.fetchSystemConfigs();

      // Assert: Kiểm tra trạng thái store.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('systemConfig.errors.fetch'); // i18n mock returns the key
      expect(store.configs).toEqual([]);
      expect(mockGetSystemConfigs).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - configs không được thay đổi.
      // - Service getSystemConfigs phải được gọi chính xác một lần.
      // - console.error phải được gọi để log lỗi.
    });
  });

  describe('updateSystemConfig', () => {
    // Mục tiêu của test: Đảm bảo rằng action updateSystemConfig xử lý thành công việc cập nhật cấu hình hệ thống.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service updateSystemConfig trả về thành công.
    it('should update system config successfully', async () => {
      const configId = '1';
      const updatedConfig: SystemConfig = { id: configId, key: 'key1', value: 'newValue1' };
      mockUpdateSystemConfig.mockResolvedValue(ok(undefined));

      // Act: Gọi action updateSystemConfig.
      await store.updateSystemConfig(configId, updatedConfig);

      // Assert: Kiểm tra trạng thái store và việc gọi service.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdateSystemConfig).toHaveBeenCalledTimes(1);
      expect(mockUpdateSystemConfig).toHaveBeenCalledWith(configId, updatedConfig);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - Service updateSystemConfig phải được gọi chính xác một lần với ID và dữ liệu cấu hình đã cho.
    });

    // Mục tiêu của test: Đảm bảo rằng action updateSystemConfig xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service updateSystemConfig trả về lỗi.
    it('should handle update system config failure', async () => {
      const configId = '1';
      const updatedConfig: SystemConfig = { id: configId, key: 'key1', value: 'newValue1' };
      const errorMessage = 'Failed to update config.';
      mockUpdateSystemConfig.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action updateSystemConfig.
      await store.updateSystemConfig(configId, updatedConfig);

      // Assert: Kiểm tra trạng thái store và việc gọi service.
      expect(store.loading).toBe(false);
      expect(store.error).toBe('systemConfig.errors.update'); // i18n mock returns the key
      expect(mockUpdateSystemConfig).toHaveBeenCalledTimes(1);
      expect(mockUpdateSystemConfig).toHaveBeenCalledWith(configId, updatedConfig);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service updateSystemConfig phải được gọi chính xác một lần với ID và dữ liệu cấu hình đã cho.
      // - console.error phải được gọi để log lỗi.
    });
  });

  describe('getSystemConfigByKey', () => {
    // Mục tiêu của test: Đảm bảo getter getSystemConfigByKey trả về cấu hình đúng khi tìm thấy.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Thiết lập trạng thái configs của store.
    it('should return the correct config by key when found', () => {
      const mockConfigs: SystemConfig[] = [
        { id: '1', key: 'key1', value: 'value1' },
        { id: '2', key: 'key2', value: 'value2' },
      ];
      store.configs = mockConfigs;

      // Act: Gọi getter với một key tồn tại.
      const config = store.getSystemConfigByKey('key1');

      // Assert: Kiểm tra giá trị trả về.
      expect(config).toEqual({ id: '1', key: 'key1', value: 'value1' });
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Getter phải trả về đối tượng cấu hình có key tương ứng.
    });

    // Mục tiêu của test: Đảm bảo getter getSystemConfigByKey trả về undefined khi không tìm thấy cấu hình.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Thiết lập trạng thái configs của store.
    it('should return undefined when config not found', () => {
      const mockConfigs: SystemConfig[] = [
        { id: '1', key: 'key1', value: 'value1' },
      ];
      store.configs = mockConfigs;

      // Act: Gọi getter với một key không tồn tại.
      const config = store.getSystemConfigByKey('nonExistentKey');

      // Assert: Kiểm tra giá trị trả về.
      expect(config).toBeUndefined();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Getter phải trả về undefined khi không tìm thấy cấu hình với key đã cho.
    });
  });
});