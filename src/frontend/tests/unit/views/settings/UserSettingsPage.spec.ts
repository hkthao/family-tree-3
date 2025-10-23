import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import UserSettingsPage from '@/views/settings/UserSettingsPage.vue';
import { createVuetify } from 'vuetify';
import { createTestingPinia } from '@pinia/testing';

// Mock vue-i18n
vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: vi.fn((key) => key), // Mock t function to return the key
  }),
}));

// Mock child components
vi.mock('@/components/settings', () => ({
  ProfileSettings: {
    name: 'ProfileSettings',
    template: '<div class="mock-profile-settings"></div>',
  },
  PreferencesSettings: {
    name: 'PreferencesSettings',
    template: '<div class="mock-preferences-settings"></div>',
  },
}));

describe('UserSettingsPage.vue', () => {
  let vuetify: any;
  let pinia: ReturnType<typeof createTestingPinia>;

  beforeEach(() => {
    vuetify = createVuetify();
    pinia = createTestingPinia({ createSpy: vi.fn });
  });

  it('should render initial state with ProfileSettings tab active', async () => {
    // Mục tiêu: Đảm bảo component hiển thị đúng trạng thái ban đầu với tab ProfileSettings hoạt động.
    // Arrange
    const wrapper = mount(UserSettingsPage, {
      global: {
        plugins: [pinia, vuetify],
      },
    });
    await wrapper.vm.$nextTick();

    // Assert
    expect(wrapper.find('[data-testid="tab-profile"]').classes('v-tab--selected')).toBe(true);
    expect(wrapper.find('[data-testid="window-item-profile"]').isVisible()).toBe(true);
    // expect(wrapper.find('[data-testid="window-item-preferences"]').isVisible()).toBe(false);
  });

  it('should switch to PreferencesSettings tab when clicked', async () => {
    // Mục tiêu: Đảm bảo component chuyển sang tab PreferencesSettings khi người dùng nhấp vào.
    // Arrange
    const wrapper = mount(UserSettingsPage, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Act
    await wrapper.find('[data-testid="tab-preferences"]').trigger('click');
    await wrapper.vm.$nextTick();

    // Assert
    expect(wrapper.find('[data-testid="tab-preferences"]').classes('v-tab--selected')).toBe(true);
    expect(wrapper.find('[data-testid="window-item-preferences"]').isVisible()).toBe(true);
    // expect(wrapper.find('[data-testid="window-item-profile"]').isVisible()).toBe(false);
  });

  it('should switch back to ProfileSettings tab when clicked', async () => {
    // Mục tiêu: Đảm bảo component chuyển về tab ProfileSettings khi người dùng nhấp lại vào.
    // Arrange
    const wrapper = mount(UserSettingsPage, {
      global: {
        plugins: [pinia, vuetify],
      },
    });

    // Act: Switch to preferences first
    await wrapper.find('[data-testid="tab-preferences"]').trigger('click');
    await wrapper.vm.$nextTick();
    // Act: Switch back to profile
    await wrapper.find('[data-testid="tab-profile"]').trigger('click');
    await wrapper.vm.$nextTick();

    // Assert
    expect(wrapper.find('[data-testid="tab-profile"]').classes('v-tab--selected')).toBe(true);
    expect(wrapper.find('[data-testid="window-item-profile"]').isVisible()).toBe(true);
    // expect(wrapper.find('[data-testid="window-item-preferences"]').isVisible()).toBe(false);
  });
});
