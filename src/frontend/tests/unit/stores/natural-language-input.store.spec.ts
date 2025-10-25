import { setActivePinia, createPinia } from 'pinia';
import { useNaturalLanguageInputStore } from '@/stores/natural-language-input.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ok, err } from '@/types';
import { RelationshipType, type Family, type Member, type Event, type Relationship, EventType } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the INaturalLanguageInputService
const mockGenerateFamilyData = vi.fn();
const mockGenerateMemberData = vi.fn();
const mockGenerateEventData = vi.fn();
const mockGenerateRelationshipData = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    naturalLanguageInput: {
      generateFamilyData: mockGenerateFamilyData,
      generateMemberData: mockGenerateMemberData,
      generateEventData: mockGenerateEventData,
      generateRelationshipData: mockGenerateRelationshipData,
    },
    // Add other services as empty objects if they are not directly used by natural-language-input.store
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
    notification: {},
    relationship: {},
    systemConfig: {},
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

describe('natural-language-input.store', () => {
  let store: ReturnType<typeof useNaturalLanguageInputStore>;

  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useNaturalLanguageInputStore();
    store.$reset();
    // Inject the mocked services
    store.services = createServices('mock');

    // Reset mocks before each test
    mockGenerateFamilyData.mockReset();
    mockGenerateMemberData.mockReset();
    mockGenerateEventData.mockReset();
    mockGenerateRelationshipData.mockReset();
  });

  it('should have correct initial state', () => {
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('generateFamilyData', () => {
    // Mục tiêu của test: Đảm bảo rằng action generateFamilyData xử lý thành công việc tạo dữ liệu gia đình.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateFamilyData trả về thành công.
    it('should generate family data successfully', async () => {
      const prompt = 'Generate a family with 3 members';
      const mockFamily: Family = { id: '1', name: 'Test Family', description: 'Desc' };
      mockGenerateFamilyData.mockResolvedValue(ok([mockFamily]));

      // Act: Gọi action generateFamilyData.
      const result = await store.generateFamilyData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toEqual([mockFamily]);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockGenerateFamilyData).toHaveBeenCalledTimes(1);
      expect(mockGenerateFamilyData).toHaveBeenCalledWith(prompt);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về dữ liệu gia đình khi tạo thành công.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - Service generateFamilyData phải được gọi chính xác một lần với prompt đã cho.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateFamilyData xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateFamilyData trả về lỗi.
    it('should handle generate family data failure from service', async () => {
      const prompt = 'Generate a family with 3 members';
      const errorMessage = 'Failed to generate family data.';
      mockGenerateFamilyData.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action generateFamilyData.
      const result = await store.generateFamilyData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateFamilyData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi tạo thất bại.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateFamilyData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateFamilyData xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateFamilyData ném ra lỗi.
    it('should handle unexpected error during generate family data', async () => {
      const prompt = 'Generate a family with 3 members';
      const errorMessage = 'Network error.';
      mockGenerateFamilyData.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action generateFamilyData.
      const result = await store.generateFamilyData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateFamilyData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi có lỗi không mong muốn.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateFamilyData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });
  });

  describe('generateMemberData', () => {
    // Mục tiêu của test: Đảm bảo rằng action generateMemberData xử lý thành công việc tạo dữ liệu thành viên.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateMemberData trả về thành công.
    it('should generate member data successfully', async () => {
      const prompt = 'Generate a member for family 1';
      const mockMember: Member = { id: '1', familyId: '1', firstName: 'John', lastName: 'Doe', fullName: 'John Doe' };
      mockGenerateMemberData.mockResolvedValue(ok([mockMember]));

      // Act: Gọi action generateMemberData.
      const result = await store.generateMemberData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toEqual([mockMember]);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockGenerateMemberData).toHaveBeenCalledTimes(1);
      expect(mockGenerateMemberData).toHaveBeenCalledWith(prompt);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về dữ liệu thành viên khi tạo thành công.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - Service generateMemberData phải được gọi chính xác một lần với prompt đã cho.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateMemberData xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateMemberData trả về lỗi.
    it('should handle generate member data failure from service', async () => {
      const prompt = 'Generate a member for family 1';
      const errorMessage = 'Failed to generate member data.';
      mockGenerateMemberData.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action generateMemberData.
      const result = await store.generateMemberData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateMemberData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi tạo thất bại.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateMemberData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateMemberData xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateMemberData ném ra lỗi.
    it('should handle unexpected error during generate member data', async () => {
      const prompt = 'Generate a member for family 1';
      const errorMessage = 'Network error.';
      mockGenerateMemberData.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action generateMemberData.
      const result = await store.generateMemberData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateMemberData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi có lỗi không mong muốn.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateMemberData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });
  });

  describe('generateEventData', () => {
    // Mục tiêu của test: Đảm bảo rằng action generateEventData xử lý thành công việc tạo dữ liệu sự kiện.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateEventData trả về thành công.
    it('should generate event data successfully', async () => {
      const prompt = 'Generate an event for family 1';
      const mockEvent: Event = { id: '1', familyId: '1', name: 'Wedding', startDate: new Date('2023-01-01'), type: EventType.Marriage };
      mockGenerateEventData.mockResolvedValue(ok([mockEvent]));

      // Act: Gọi action generateEventData.
      const result = await store.generateEventData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toEqual([mockEvent]);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockGenerateEventData).toHaveBeenCalledTimes(1);
      expect(mockGenerateEventData).toHaveBeenCalledWith(prompt);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về dữ liệu sự kiện khi tạo thành công.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - Service generateEventData phải được gọi chính xác một lần với prompt đã cho.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateEventData xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateEventData trả về lỗi.
    it('should handle generate event data failure from service', async () => {
      const prompt = 'Generate an event for family 1';
      const errorMessage = 'Failed to generate event data.';
      mockGenerateEventData.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action generateEventData.
      const result = await store.generateEventData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateEventData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi tạo thất bại.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateEventData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateEventData xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateEventData ném ra lỗi.
    it('should handle unexpected error during generate event data', async () => {
      const prompt = 'Generate an event for family 1';
      const errorMessage = 'Network error.';
      mockGenerateEventData.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action generateEventData.
      const result = await store.generateEventData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateEventData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi có lỗi không mong muốn.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateEventData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });
  });

  describe('generateRelationshipData', () => {
    // Mục tiêu của test: Đảm bảo rằng action generateRelationshipData xử lý thành công việc tạo dữ liệu quan hệ.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateRelationshipData trả về thành công.
    it('should generate relationship data successfully', async () => {
      const prompt = 'Generate a relationship between John and Jane';
      const mockRelationship: Relationship = { id: '1', familyId: '1', sourceMemberId: '1', targetMemberId: '2', type: RelationshipType.Wife };
      mockGenerateRelationshipData.mockResolvedValue(ok([mockRelationship]));

      // Act: Gọi action generateRelationshipData.
      const result = await store.generateRelationshipData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toEqual([mockRelationship]);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockGenerateRelationshipData).toHaveBeenCalledTimes(1);
      expect(mockGenerateRelationshipData).toHaveBeenCalledWith(prompt);
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về dữ liệu quan hệ khi tạo thành công.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Không có lỗi nào được ghi nhận.
      // - Service generateRelationshipData phải được gọi chính xác một lần với prompt đã cho.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateRelationshipData xử lý thất bại từ service.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateRelationshipData trả về lỗi.
    it('should handle generate relationship data failure from service', async () => {
      const prompt = 'Generate a relationship between John and Jane';
      const errorMessage = 'Failed to generate relationship data.';
      mockGenerateRelationshipData.mockResolvedValue(err({ message: errorMessage } as ApiError));

      // Act: Gọi action generateRelationshipData.
      const result = await store.generateRelationshipData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateRelationshipData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi tạo thất bại.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateRelationshipData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });

    // Mục tiêu của test: Đảm bảo rằng action generateRelationshipData xử lý lỗi không mong muốn.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo store, mock service generateRelationshipData ném ra lỗi.
    it('should handle unexpected error during generate relationship data', async () => {
      const prompt = 'Generate a relationship between John and Jane';
      const errorMessage = 'Network error.';
      mockGenerateRelationshipData.mockRejectedValue(new Error(errorMessage));

      // Act: Gọi action generateRelationshipData.
      const result = await store.generateRelationshipData(prompt);

      // Assert: Kiểm tra trạng thái store và giá trị trả về.
      expect(result).toBeNull();
      expect(store.loading).toBe(false);
      expect(store.error).toBe('aiInput.generateError'); // i18n mock returns the key
      expect(mockGenerateRelationshipData).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
      // Giải thích vì sao kết quả mong đợi là đúng:
      // - Action phải trả về null khi có lỗi không mong muốn.
      // - Trạng thái loading phải được đặt thành false sau khi hoàn thành.
      // - Lỗi phải được ghi nhận trong store.
      // - Service generateRelationshipData phải được gọi chính xác một lần với prompt đã cho.
      // - console.error phải được gọi để log lỗi.
    });
  });
});
