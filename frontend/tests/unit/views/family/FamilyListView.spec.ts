import { mount, flushPromises } from '@vue/test-utils';
import { describe, it, expect, vi } from 'vitest';
import FamilyListView from '@/views/family/FamilyListView.vue';
import { useFamilyStore } from '@/stores/family.store';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import { createI18n } from 'vue-i18n';
import { createVuetify } from 'vuetify';
import { createRouter, createWebHistory } from 'vue-router';
import { generateMockFamilies } from '@/data/mock/family.mock';

// Mock ResizeObserver
global.ResizeObserver = vi.fn(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));

const mockFamilies = generateMockFamilies(5);

// Mock the stores
vi.mock('@/stores/family.store', () => ({
  useFamilyStore: vi.fn(() => ({
    families: mockFamilies,
    totalItems: mockFamilies.length,
    loading: false,
    searchFamilies: vi.fn(),
    deleteFamily: vi.fn(),
    _loadFamilies: vi.fn(),
    setPage: vi.fn(),
    setItemsPerPage: vi.fn(),
  })),
}));

vi.mock('@/stores/member.store', () => ({
  useMemberStore: vi.fn(() => ({
    members: [],
    fetchMembers: vi.fn(),
  })),
}));

vi.mock('@/stores/notification.store', () => ({
  useNotificationStore: vi.fn(() => ({
    snackbar: { show: false, message: '', color: '' },
    showSnackbar: vi.fn(),
  })),
}));

const i18n = createI18n({
  legacy: false,
  locale: 'en',
  messages: {
    en: {},
  },
});

const vuetify = createVuetify();

const router = createRouter({
  history: createWebHistory(),
  routes: [{ path: '/', component: { template: '' } }],
});

describe('FamilyListView.vue', () => {
  it('renders without errors', () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router], // Add router plugin
      },
    });
    expect(wrapper.exists()).toBe(true);
  });

  // Test to ensure that loadFamilies function calls the store action and updates the component
  it('loads families when loadFamilies is called', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });

    // Manually trigger the loadFamilies function
    await (wrapper.vm as any).loadFamilies();

    // Check if the families are rendered in the component
    const familyList = wrapper.findComponent({ name: 'FamilyList' });
    expect(familyList.props('families')).toEqual(mockFamilies);
  });
});
