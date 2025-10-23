import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import FamilyAddView from '@/views/family/FamilyAddView.vue';
import { createTestingPinia } from '@pinia/testing';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';
import { createVuetify } from 'vuetify';

// Mock vue-router
const mockPush = vi.fn();
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock vue-i18n
vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: vi.fn((key) => key), // Mock t function to return the key
  }),
}));

vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: vi.fn((key) => key),
    },
  },
}));

// Mock the FamilyForm component
vi.mock('@/components/family', () => ({
  FamilyForm: {
    template: '<div class="mock-family-form"></div>',
    props: ['family'],
    methods: {
      validate: vi.fn(),
      getFormData: vi.fn(),
    },
  },
}));

describe('FamilyAddView.vue', () => {
  let familyStore: ReturnType<typeof useFamilyStore>;
  let notificationStore: ReturnType<typeof useNotificationStore>;
  let vuetify: any;

  beforeEach(() => {
    vi.clearAllMocks();
    const pinia = createTestingPinia({
      createSpy: vi.fn,
    });
    familyStore = useFamilyStore(pinia);
    notificationStore = useNotificationStore(pinia);

    // Initialize Vuetify
    vuetify = createVuetify();
  });

  it('should add a family and show success notification on successful save', async () => {
    // Mục tiêu của test: Đảm bảo rằng khi người dùng thêm một gia đình thành công, 
    // thông báo thành công sẽ hiển thị và ứng dụng sẽ điều hướng về trang danh sách gia đình.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component, mock các store và FamilyForm để simulate hành vi thành công.
    const wrapper = mount(FamilyAddView, {
      global: {
        plugins: [createTestingPinia({ createSpy: vi.fn }), vuetify],
      },
    });

    // Get the mocked store instance from the wrapper's global pinia instance
    const familyStore = useFamilyStore(); // This will return the mocked store
    const notificationStore = useNotificationStore(); // This will return the mocked store

    const familyForm = wrapper.findComponent(FamilyForm);
    // Set the ref value
    (wrapper.vm as any).familyFormRef = familyForm.vm;

    (familyForm.vm as any).validate = vi.fn().mockResolvedValue(true);
    (familyForm.vm as any).getFormData = vi.fn().mockReturnValue({
      name: 'Test Family',
      description: 'A family for testing',
    });
    (familyStore.addItem as any).mockResolvedValue(undefined); // Mock successful addItem

    // Act: Kích hoạt sự kiện click vào nút "Lưu".
    await wrapper.findAll('button')[1].trigger('click'); // Assuming save is the second button

    // Assert: Kiểm tra các hành vi mong đợi.
    expect(familyForm.vm.validate).toHaveBeenCalled();
    expect(familyForm.vm.getFormData).toHaveBeenCalled();
    expect(familyStore.addItem).toHaveBeenCalledWith({
      name: 'Test Family',
      description: 'A family for testing',
    });
    expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      'family.management.messages.addSuccess',
      'success',
    );
    expect(mockPush).toHaveBeenCalledWith('/family');
    // Giải thích vì sao kết quả mong đợi là đúng:
    // - validate() và getFormData() của FamilyForm phải được gọi để lấy dữ liệu và xác thực.
    // - addItem() của familyStore phải được gọi với dữ liệu form đã lấy.
    // - showSnackbar() của notificationStore phải được gọi với thông báo thành công.
    // - mockPush() của vue-router phải được gọi để điều hướng về trang danh sách.
  });

  it('should navigate to /family when cancel button is clicked', async () => {
    // Mục tiêu của test: Đảm bảo rằng khi người dùng nhấn nút "Hủy",
    // ứng dụng sẽ điều hướng về trang danh sách gia đình mà không thực hiện thêm hành động nào khác.

    // Các bước (Arrange, Act, Assert):
    // Arrange: Khởi tạo component.
    const wrapper = mount(FamilyAddView, {
      global: {
        plugins: [createTestingPinia({ createSpy: vi.fn }), vuetify],
      },
    });

    // Act: Kích hoạt sự kiện click vào nút "Hủy".
    await wrapper.findAll('button')[0].trigger('click'); // Assuming cancel is the first button

    // Assert: Kiểm tra rằng mockPush đã được gọi với đường dẫn chính xác.
    expect(mockPush).toHaveBeenCalledWith('/family');
    // Giải thích vì sao kết quả mong đợi là đúng:
    // - mockPush() của vue-router phải được gọi với đường dẫn '/family' để xác nhận điều hướng.
    // - Không có store action nào khác được gọi vì hành động chỉ là hủy bỏ.
  });
});