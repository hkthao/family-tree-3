
import { mount, flushPromises } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi, type Mock } from 'vitest';
import { ref } from 'vue';
import FamilyEditView from '@/views/family/FamilyEditView.vue';
import { createTestingPinia } from '@pinia/testing';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FamilyForm } from '@/components/family';
import { createVuetify } from 'vuetify';
import type { Family } from '@/types';
import { FamilyVisibility } from '@/types';

// Mock vue-router
const mockPush = vi.fn();
const route = ref({
  params: { id: '1' },
});
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  useRoute: () => route.value,
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
let mockFamilyFormValidate: Mock;
let mockFamilyFormGetFormData: Mock;

vi.mock('@/components/family', () => ({
  FamilyForm: {
    template: '<div class="mock-family-form"></div>',
    props: ['initialFamilyData', 'readOnly'],
    methods: {
      validate: () => mockFamilyFormValidate(),
      getFormData: () => mockFamilyFormGetFormData(),
    },
  },
}));

describe('FamilyEditView.vue', () => {
  let vuetify: any;
  let familyStore: ReturnType<typeof useFamilyStore>;
  let notificationStore: ReturnType<typeof useNotificationStore>;
  let pinia: ReturnType<typeof createTestingPinia>;

  const mockFamily: Family = {
    id: '1',
    name: 'Test Family',
    description: 'A family for testing',
    visibility: FamilyVisibility.Public,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    vuetify = createVuetify();
    pinia = createTestingPinia({ createSpy: vi.fn });
    familyStore = useFamilyStore(pinia);
    notificationStore = useNotificationStore(pinia);

    // Reset mocks for FamilyForm methods
    mockFamilyFormValidate = vi.fn();
    mockFamilyFormGetFormData = vi.fn();

    // Mock store actions
    (familyStore.getById as Mock).mockImplementation(async (id) => {
      if (id === mockFamily.id) {
        familyStore.currentItem = mockFamily;
        familyStore.error = null;
      } else {
        familyStore.currentItem = null;
        familyStore.error = 'Error';
      }
    });
    (familyStore.updateItem as Mock).mockResolvedValue(undefined);
  });

  it('should fetch family data on mount and display form', async () => {
    // Mục tiêu: Đảm bảo component lấy dữ liệu gia đình khi được mount và hiển thị form.
    // Arrange
    const wrapper = mount(FamilyEditView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Act
    await flushPromises();

    // Assert
    expect(familyStore.getById).toHaveBeenCalledWith('1');
    expect(wrapper.findComponent(FamilyForm).exists()).toBe(true);
    expect(wrapper.findComponent(FamilyForm).props('initialFamilyData')).toEqual(
      mockFamily,
    );
  });

  it('should update family and show success notification on successful save', async () => {
    // Mục tiêu: Đảm bảo khi lưu thành công, thông báo thành công hiển thị và điều hướng.
    // Arrange
    const wrapper = mount(FamilyEditView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();

    const familyForm = wrapper.findComponent(FamilyForm);
    (wrapper.vm as any).familyFormRef = familyForm.vm;

        mockFamilyFormValidate.mockResolvedValue(true);
        const updatedFamilyData = { ...mockFamily, name: 'Updated Family' };
        mockFamilyFormGetFormData.mockReturnValue(updatedFamilyData);
    
        // Ensure the ref is correctly set before the action
        (wrapper.vm as any).familyFormRef = { 
          validate: mockFamilyFormValidate, 
          getFormData: mockFamilyFormGetFormData 
        };    // Act
    await wrapper.find('[data-testid="button-save"]').trigger('click'); // Click Save button
    await flushPromises();

    // Assert
    expect(mockFamilyFormValidate).toHaveBeenCalled();
    expect(mockFamilyFormGetFormData).toHaveBeenCalled();
    expect(familyStore.updateItem).toHaveBeenCalledWith(updatedFamilyData);
    expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      'family.management.messages.updateSuccess',
      'success',
    );
    expect(mockPush).toHaveBeenCalledWith('/family');
  });

  it('should not update family if validation fails', async () => {
    // Mục tiêu: Đảm bảo không có hành động cập nhật nào được thực hiện nếu validation thất bại.
    // Arrange
    const wrapper = mount(FamilyEditView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();

    const familyForm = wrapper.findComponent(FamilyForm);
    (wrapper.vm as any).familyFormRef = familyForm.vm;

    // Act
    await wrapper.find('[data-testid="button-save"]').trigger('click'); // Click Save button
    await flushPromises();

    // Assert
    expect(mockFamilyFormValidate).toHaveBeenCalled();
    expect(mockFamilyFormGetFormData).not.toHaveBeenCalled();
    expect(familyStore.updateItem).not.toHaveBeenCalled();
    expect(notificationStore.showSnackbar).not.toHaveBeenCalled();
    expect(mockPush).not.toHaveBeenCalled();
  });

  it('should show error notification if update fails', async () => {
    // Mục tiêu: Đảm bảo thông báo lỗi hiển thị nếu việc cập nhật trên store thất bại.
    // Arrange
    const errorMessage = 'Update failed';
    (familyStore.updateItem as Mock).mockImplementation(async () => {
      familyStore.error = errorMessage;
    });

    const wrapper = mount(FamilyEditView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();

    const familyForm = wrapper.findComponent(FamilyForm);
    (wrapper.vm as any).familyFormRef = familyForm.vm;

    mockFamilyFormValidate.mockResolvedValue(true);
    mockFamilyFormGetFormData.mockReturnValue(mockFamily);

    // Act
    await wrapper.find('[data-testid="button-save"]').trigger('click'); // Click Save button
    await flushPromises();

    // Assert
    expect(familyStore.updateItem).toHaveBeenCalledWith(mockFamily);
    expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      errorMessage,
      'error',
    );
    expect(mockPush).not.toHaveBeenCalled();
  });

  it('should navigate to /family when cancel button is clicked', async () => {
    // Mục tiêu: Đảm bảo nút "Hủy" điều hướng người dùng về trang danh sách.
    // Arrange
    const wrapper = mount(FamilyEditView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await flushPromises();

    // Act
    await wrapper.find('[data-testid="button-cancel"]').trigger('click'); // Click Cancel button

    // Assert
    expect(mockPush).toHaveBeenCalledWith('/family');
  });
});
