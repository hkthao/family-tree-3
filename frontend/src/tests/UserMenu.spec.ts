import { mount } from '@vue/test-utils';
import { createVuetify } from 'vuetify';
import { createI18n } from 'vue-i18n';
import UserMenu from '@/components/layout/UserMenu.vue';
import { mockUser } from '@/data/userMock';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import en from '@/locales/en.json';
import vi from '@/locales/vi.json';

const vuetify = createVuetify();
const i18n = createI18n({
  legacy: false,
  locale: 'en',
  messages: { en, vi },
});

global.ResizeObserver = require('resize-observer-polyfill');

describe('UserMenu.vue', () => {
  let wrapper;

  beforeEach(() => {
    wrapper = mount(UserMenu, {
      props: { currentUser: mockUser, notificationsCount: 5 },
      global: {
        plugins: [vuetify, i18n],
        stubs: { 'router-link': true } // Stub router-link for testing
      },
    });
  });

  it('renders avatar and online dot when online=true', async () => {
    expect(wrapper.find('.user-avatar').exists()).toBe(true);
    expect(wrapper.find('.online-indicator').exists()).toBe(true);
  });

  it('opens menu on click and shows header info', async () => {
    await wrapper.find('.user-avatar').trigger('click');
    expect(wrapper.vm.menuOpen).toBe(true);
    expect(wrapper.find('.user-menu-header').exists()).toBe(true);
    expect(wrapper.html()).toContain(mockUser.name);
    expect(wrapper.html()).toContain(mockUser.role);
  });

  it('emits navigate with correct route when clicking Profile', async () => {
    await wrapper.find('.user-avatar').trigger('click'); // Open menu
    await wrapper.find('.user-menu-item').trigger('click'); // Click first item (Profile)
    expect(wrapper.emitted().navigate).toBeTruthy();
    expect(wrapper.emitted().navigate[0][0]).toBe('/profile');
  });

  it('shows confirm dialog on logout click and emits logout after confirm', async () => {
    await wrapper.find('.user-avatar').trigger('click'); // Open menu
    await wrapper.find('[prepend-icon="mdi-logout"]').trigger('click'); // Click logout
    expect(wrapper.vm.confirmLogoutDialog).toBe(true);

    await wrapper.find('.v-card-actions .v-btn--error').trigger('click'); // Click confirm logout
    expect(wrapper.emitted().logout).toBeTruthy();
    expect(wrapper.vm.confirmLogoutDialog).toBe(false);
  });

  it('closes menu when clicking outside', async () => {
    await wrapper.find('.user-avatar').trigger('click'); // Open menu
    expect(wrapper.vm.menuOpen).toBe(true);
    await wrapper.find('.v-overlay__scrim').trigger('click'); // Click outside
    expect(wrapper.vm.menuOpen).toBe(false);
  });

  it('handles keyboard navigation (Escape to close)', async () => {
    await wrapper.find('.user-avatar').trigger('click'); // Open menu
    expect(wrapper.vm.menuOpen).toBe(true);
    await wrapper.trigger('keydown.esc');
    expect(wrapper.vm.menuOpen).toBe(false);
  });

  it('shows initials if no avatarUrl is provided', async () => {
    const userWithoutAvatar = { ...mockUser, avatarUrl: undefined, name: 'Jane Doe' };
    wrapper = mount(UserMenu, {
      props: { currentUser: userWithoutAvatar },
      global: {
        plugins: [vuetify, i18n],
        stubs: { 'router-link': true }
      },
    });
    expect(wrapper.find('.v-avatar span').text()).toBe('JD');
  });

  it('shows notification badge if notificationsCount > 0', async () => {
    expect(wrapper.find('.v-badge').exists()).toBe(true);
    expect(wrapper.find('.v-badge__content').text()).toBe('5');
  });

  it('does not show notification badge if notificationsCount is 0', async () => {
    wrapper = mount(UserMenu, {
      props: { currentUser: mockUser, notificationsCount: 0 },
      global: {
        plugins: [vuetify, i18n],
        stubs: { 'router-link': true }
      },
    });
    expect(wrapper.find('.v-badge').exists()).toBe(false);
  });
});
