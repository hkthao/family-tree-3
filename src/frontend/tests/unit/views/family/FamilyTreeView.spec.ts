
import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import FamilyTreeView from '@/views/family/FamilyTreeView.vue';
import { createVuetify } from 'vuetify';
import { createTestingPinia } from '@pinia/testing';

// Mock vue-i18n
vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: vi.fn((key) => key), // Mock t function to return the key
  }),
}));

// Mock child components
vi.mock('@/components/common/FamilyAutocomplete.vue', () => ({
  default: {
    name: 'FamilyAutocomplete',
    template: '<div class="mock-family-autocomplete"></div>',
    props: ['modelValue', 'label', 'clearable', 'hideDetails'],
    emits: ['update:modelValue'],
  },
}));

vi.mock('@/components/family', () => ({
  TreeChart: {
    name: 'TreeChart',
    template: '<div class="mock-tree-chart"></div>',
    props: ['familyId'],
  },
}));

describe('FamilyTreeView.vue', () => {
  let vuetify: any;
  let pinia: ReturnType<typeof createTestingPinia>;

  beforeEach(() => {
    vuetify = createVuetify();
    pinia = createTestingPinia({ createSpy: vi.fn });
  });

  it('should render initial state correctly', () => {
    // Mục tiêu: Đảm bảo component hiển thị đúng trạng thái ban đầu, yêu cầu người dùng chọn một gia đình.
    // Arrange
    const wrapper = mount(FamilyTreeView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Assert
    expect(wrapper.text()).toContain('family.tree.title');
    expect(wrapper.findComponent({ name: 'FamilyAutocomplete' }).exists()).toBe(true);
    expect(wrapper.text()).toContain('event.messages.selectFamily');
    expect(wrapper.findComponent({ name: 'TreeChart' }).exists()).toBe(false);
  });

  it('should display TreeChart when a family is selected', async () => {
    // Mục tiêu: Đảm bảo TreeChart được hiển thị khi người dùng chọn một gia đình từ Autocomplete.
    // Arrange
    const wrapper = mount(FamilyTreeView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    const autocomplete = wrapper.findComponent({ name: 'FamilyAutocomplete' });

    // Act
    await autocomplete.vm.$emit('update:modelValue', 'family-123');

    // Assert
    expect(wrapper.findComponent({ name: 'TreeChart' }).exists()).toBe(true);
    expect(wrapper.findComponent({ name: 'TreeChart' }).props('familyId')).toBe(
      'family-123',
    );
    expect(wrapper.text()).not.toContain('event.messages.selectFamily');
  });

  it('should hide TreeChart when family selection is cleared', async () => {
    // Mục tiêu: Đảm bảo TreeChart bị ẩn và thông báo yêu cầu chọn lại xuất hiện khi lựa chọn gia đình bị xóa.
    // Arrange
    const wrapper = mount(FamilyTreeView, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    const autocomplete = wrapper.findComponent({ name: 'FamilyAutocomplete' });

    // Act: Select a family first
    await autocomplete.vm.$emit('update:modelValue', 'family-123');

    // Assert: TreeChart is visible
    expect(wrapper.findComponent({ name: 'TreeChart' }).exists()).toBe(true);

    // Act: Clear the selection
    await autocomplete.vm.$emit('update:modelValue', null);

    // Assert: TreeChart is hidden and message is shown
    expect(wrapper.findComponent({ name: 'TreeChart' }).exists()).toBe(false);
    expect(wrapper.text()).toContain('event.messages.selectFamily');
  });
});
