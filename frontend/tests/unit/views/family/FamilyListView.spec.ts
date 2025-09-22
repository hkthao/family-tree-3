import { mount } from '@vue/test-utils';
import { describe, it, expect, vi } from 'vitest';
import FamilyListView from '@/views/family/FamilyListView.vue';
import { useFamilyStore } from '@/stores/family.store';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import { createI18n } from 'vue-i18n';
import { createVuetify } from 'vuetify';

// Mock ResizeObserver
global.ResizeObserver = vi.fn(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));

// Mock the stores
vi.mock('@/stores/family.store', () => ({
  useFamilyStore: vi.fn(() => ({
    families: [],
    totalItems: 0,
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

describe('FamilyListView.vue', () => {
  it('renders without errors', () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify], // Add i18n and vuetify plugins
      },
    });
    expect(wrapper.exists()).toBe(true);
  });
});
