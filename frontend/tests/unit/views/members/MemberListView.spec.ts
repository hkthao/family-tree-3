import { mount } from '@vue/test-utils';
import { describe, it, expect, vi } from 'vitest';
import MemberListView from '@/views/members/MemberListView.vue';
import { useMemberStore } from '@/stores/member.store';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import { createI18n } from 'vue-i18n';
import { createVuetify } from 'vuetify';
import { createRouter, createWebHistory } from 'vue-router';

// Mock ResizeObserver
global.ResizeObserver = vi.fn(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));

// Mock the stores
vi.mock('@/stores/member.store', () => ({
  useMemberStore: vi.fn(() => ({
    members: [],
    filteredMembers: [],
    loading: false,
    fetchMembers: vi.fn(),
    searchMembers: vi.fn(),
    deleteMember: vi.fn(),
    setPage: vi.fn(),
    setItemsPerPage: vi.fn(),
  })),
}));

vi.mock('@/stores/family.store', () => ({
  useFamilyStore: vi.fn(() => ({
    families: [],
    searchFamilies: vi.fn(),
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

describe('MemberListView.vue', () => {
  it('renders without errors', () => {
    const wrapper = mount(MemberListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    expect(wrapper.exists()).toBe(true);
  });
});
