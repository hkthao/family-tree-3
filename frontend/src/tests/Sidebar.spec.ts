import { mount } from '@vue/test-utils';
import { createVuetify } from 'vuetify';
import { createI18n } from 'vue-i18n';
import Sidebar from '@/components/layout/Sidebar.vue';
import { describe, it, expect, beforeEach } from 'vitest';
import en from '@/locales/en.json';
import vi from '@/locales/vi.json';

const vuetify = createVuetify();
const i18n = createI18n({
  legacy: false,
  locale: 'en',
  messages: { en, vi },
});

global.ResizeObserver = require('resize-observer-polyfill');

describe('Sidebar.vue', () => {
  let wrapper;

  const createWrapper = (currentUser) => {
    return mount(Sidebar, {
      props: { currentUser },
      global: {
        plugins: [vuetify, i18n],
        stubs: { 'router-link': true }
      },
    });
  };

  it('renders admin-only items for Admin user', () => {
    const adminUser = { id: 'a1', name: 'Admin', roles: ['Admin'] };
    wrapper = createWrapper(adminUser);
    const adminMenuText = i18n.global.t('admin.users');
    expect(wrapper.html()).toContain(adminMenuText);
  });

  it('does not render admin-only items for FamilyManager user', () => {
    const managerUser = { id: 'm1', name: 'Manager', roles: ['FamilyManager'] };
    wrapper = createWrapper(managerUser);
    const adminMenuText = i18n.global.t('admin.users');
    expect(wrapper.html()).not.toContain(adminMenuText);
  });

  it('filters menu items based on search query', async () => {
    const user = { id: 'u1', name: 'Test', roles: ['Admin'] };
    wrapper = createWrapper(user);
    await wrapper.find('input[type="text"]').setValue('User Management');
    expect(wrapper.html()).toContain('User Management');
    expect(wrapper.html()).not.toContain('Dashboard');
  });

  it('toggles mini variant on button click', async () => {
    const user = { id: 'u1', name: 'Test', roles: ['Admin'] };
    wrapper = createWrapper(user);
    expect(wrapper.vm.mini).toBe(false);
    await wrapper.find('.v-navigation-drawer__append .v-btn').trigger('click');
    expect(wrapper.vm.mini).toBe(true);
  });

  it('loads collapsed state from localStorage', () => {
    localStorage.setItem('app.sidebarCollapsed', 'true');
    const user = { id: 'u1', name: 'Test', roles: ['Admin'] };
    wrapper = createWrapper(user);
    expect(wrapper.vm.mini).toBe(true);
    localStorage.removeItem('app.sidebarCollapsed'); // cleanup
  });

  it('saves favorites to localStorage', async () => {
    const user = { id: 'u1', name: 'Test', roles: ['Admin'] };
    wrapper = createWrapper(user);
    await wrapper.find('[aria-label="Add to favorites"]').trigger('click');
    const favorites = JSON.parse(localStorage.getItem('app.sidebarFavorites'));
    expect(favorites).toHaveLength(1);
    expect(favorites[0].titleKey).toBe('dashboard.overview');
    localStorage.removeItem('app.sidebarFavorites'); // cleanup
  });
});