import { setActivePinia, createPinia } from 'pinia';
import { useFileUploadStore } from '@/stores/file-upload.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ok } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock the IFileUploadService
const mockUploadFile = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    fileUpload: {
      uploadFile: mockUploadFile,
    },
    // Add other services as empty objects if they are not directly used by file-upload.store
    ai: {},
    auth: {},
    chat: {},
    chunk: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    family: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

describe('file-upload.store', () => {
  let store: ReturnType<typeof useFileUploadStore>;

  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useFileUploadStore();
    store.$reset();
    // Inject the mocked services
    store.services = createServices('test');

    // Reset mocks before each test
    mockUploadFile.mockReset();

    // Set default mock resolved values
    mockUploadFile.mockResolvedValue(ok('mock-uploaded-url'));
  });

  it('should have correct initial state', () => {
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.uploadedUrl).toBeNull();
  });

  describe('uploadFile', () => {
    // Mục tiêu của test: Đảm bảo rằng action uploadFile xử lý thành công việc tải lên file.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service uploadFile trả về thành công.
    it('should upload a file successfully', async () => {
      const file = new File(['dummy'], 'test.jpg', { type: 'image/jpeg' });
      const mockUrl = 'http://example.com/test.jpg';
      mockUploadFile.mockResolvedValue(ok(mockUrl));

      // Act: Gọi action uploadFile.
      const result = await store.uploadFile(file);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.uploadedUrl).toBe(mockUrl);
      expect(mockUploadFile).toHaveBeenCalledTimes(1);
      expect(mockUploadFile).toHaveBeenCalledWith(file);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về true khi upload thành công.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - uploadedUrl phải được cập nhật với URL trả về từ service.
      // - Service uploadFile phải được gọi chính xác một lần với file đã cho.
    });
  });

  describe('reset', () => {
    // Mục tiêu của test: Đảm bảo rằng action reset đặt lại tất cả các trạng thái về giá trị ban đầu.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Thay đổi trạng thái của store để không phải là trạng thái ban đầu.
    it('should reset the store state', () => {
      store.loading = true;
      store.error = 'Some error';
      store.uploadedUrl = 'http://example.com/some.jpg';

      // Act: Gọi action reset.
      store.reset();

      // Assert: Kiểm tra rằng tất cả các trạng thái đã được đặt lại.
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.uploadedUrl).toBeNull();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - loading, error và uploadedUrl phải trở về giá trị mặc định của chúng (false, null, null).
    });
  });
});
