import { mount, flushPromises } from '@vue/test-utils';
import { describe, it, expect, vi, beforeEach } from 'vitest';
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

import { createPinia, setActivePinia } from 'pinia';
import { createServices } from '@/services/service.factory';
import type { IFamilyService } from '@/services/family/family.service.interface';
import type { IMemberService } from '@/services/member/member.service.interface';
import type { Family } from '@/types/family';
import type { Member } from '@/types/member';
import type { Paginated } from '@/types/pagination';
import { simulateLatency } from '@/utils/mockUtils';

// Mock services
class MockFamilyServiceForTest implements IFamilyService {
  private _items: Family[] = generateMockFamilies(5); // Use a private variable

  get items(): Family[] {
    return [...this._items];
  }

  async fetch(): Promise<Family[]> {
    return simulateLatency(this.items);
  }
  async getById(id: string): Promise<Family | undefined> {
    return simulateLatency(this.items.find((f) => f.id === id));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Family> {
    const familyToAdd = { ...newItem, id: 'new-family-id' };
    this._items.push(familyToAdd);
    return simulateLatency(familyToAdd);
  }
  async update(updatedItem: Family): Promise<Family> {
    const index = this._items.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this._items[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Family not found');
  }
  async delete(id: string): Promise<void> {
    const initialLength = this._items.length;
    this._items = this._items.filter((f) => f.id !== id);
    if (this._items.length === initialLength) {
      throw new Error('Family not found');
    }
    return simulateLatency(undefined);
  }

  async searchItems(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number,
  ): Promise<Paginated<Family>> {
    let filtered = this._items;

    if (searchQuery) {
      const lowerCaseSearchQuery = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (family.description &&
            family.description.toLowerCase().includes(lowerCaseSearchQuery)),
      );
    }

    if (visibility !== 'all') {
      filtered = filtered.filter((family) => family.visibility === visibility);
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return simulateLatency({
      items,
      totalItems,
      totalPages,
    });
  }
}

class MockMemberServiceForTest implements IMemberService {
  private _items: Member[] = []; // No members initially

  get items(): Member[] {
    return [...this._items];
  }

  async fetch(): Promise<Member[]> {
    return simulateLatency(this.items);
  }
  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    return simulateLatency(this.items.filter(m => m.familyId === familyId));
  }
  async getById(id: string): Promise<Member | undefined> {
    return simulateLatency(this.items.find((m) => m.id === id));
  }
  async add(newItem: Omit<Member, 'id'>): Promise<Member> {
    const memberToAdd = { ...newItem, id: 'new-member-id' };
    this._items.push(memberToAdd);
    return simulateLatency(memberToAdd);
  }
  async update(updatedItem: Member): Promise<Member> {
    const index = this._items.findIndex((m) => m.id === updatedItem.id);
    if (index !== -1) {
      this._items[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Member not found');
  }
  async delete(id: string): Promise<void> {
    const initialLength = this._items.length;
    this._items = this._items.filter((m) => m.id !== id);
    if (this._items.length === initialLength) {
      throw new Error('Member not found');
    }
    return simulateLatency(undefined);
  }
  async searchMembers(filters: any): Promise<Member[]> {
    // Simplified search for testing
    return simulateLatency(this.items.filter(m => m.fullName?.includes(filters.fullName || '')));
  }
}

// Mock the stores


  let familyStore: ReturnType<typeof useFamilyStore>;
  let memberStore: ReturnType<typeof useMemberStore>;
  let notificationStore: ReturnType<typeof useNotificationStore>;
  let mockFamilyService: MockFamilyServiceForTest;
  let mockMemberService: MockMemberServiceForTest;

  beforeEach(async () => {
    const pinia = createPinia();
    setActivePinia(pinia);

    mockFamilyService = new MockFamilyServiceForTest();
    mockMemberService = new MockMemberServiceForTest();

    familyStore = useFamilyStore();
    memberStore = useMemberStore();
    notificationStore = useNotificationStore();

    familyStore.$reset();
    memberStore.$reset();
    notificationStore.$reset();

    familyStore.services = createServices('test', { family: mockFamilyService });
    memberStore.services = createServices('test', { member: mockMemberService });

    vi.spyOn(familyStore, 'searchItems'); // Moved this line here

    await familyStore._loadItems(); // This is where the error occurs
  });

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

    await flushPromises(); // Ensure component is mounted and async operations are done

    // Manually trigger the loadFamilies function
    await (wrapper.vm as any).loadFamilies(); // Add this line back

    // Check if the familyStore.searchItems is called
    expect(familyStore.searchItems).toHaveBeenCalled();

    // Check if the items are rendered in the component
    const familyList = wrapper.findComponent({ name: 'FamilyList' });
    expect(familyList.props('items')).toEqual(mockFamilyService.items);
  });
});
