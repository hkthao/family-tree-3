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
import type { Member } from '@/types/family';
import type { Paginated, Result } from '@/types/common';
import { ok, err } from '@/types/common';
import { simulateLatency } from '@/utils/mockUtils';
import type { ApiError } from '@/utils/api';
import type { MemberFilter } from '@/services/member/member.service.interface';
import type { FamilySearchFilter } from '@/types/family';

// Mock services
class MockFamilyServiceForTest implements IFamilyService {
  private _items: Family[] = generateMockFamilies(5); // Use a private variable

  get items(): Family[] {
    return [...this._items];
  }

  async fetch(): Promise<Result<Family[], ApiError>> {
    return ok(await simulateLatency(this.items));
  }
  async getById(id: string): Promise<Result<Family | undefined, ApiError>> {
    return ok(await simulateLatency(this.items.find((f) => f.id === id)));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    const familyToAdd = { ...newItem, id: 'new-family-id' };
    this._items.push(familyToAdd);
    return ok(await simulateLatency(familyToAdd));
  }
  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    const index = this._items.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this._items[index] = updatedItem;
      return ok(await simulateLatency(updatedItem));
    }
    return err({ message: 'Family not found', statusCode: 404 });
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    const initialLength = this._items.length;
    this._items = this._items.filter((f) => f.id !== id);
    if (this._items.length === initialLength) {
      return err({ message: 'Family not found', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }

  async searchItems(
    filter: FamilySearchFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    let filtered = this._items;

    if (filter.searchQuery) {
      const lowerCaseSearchQuery = filter.searchQuery.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (family.description &&
            family.description.toLowerCase().includes(lowerCaseSearchQuery)),
      );
    }

    if (filter.visibility && filter.visibility !== 'all') {
      filtered = filtered.filter((family) => family.visibility === filter.visibility);
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return ok(await simulateLatency({
      items,
      totalItems,
      totalPages,
    }));
  }
}

class MockMemberServiceForTest implements IMemberService {
  private _items: Member[] = []; // No members initially

  get items(): Member[] {
    return [...this._items];
  }

  async fetch(): Promise<Result<Member[], ApiError>> {
    return ok(await simulateLatency(this.items));
  }
  async fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>> {
    const filteredItems = this.items.filter(member => member.familyId === familyId);
    return ok(await simulateLatency(filteredItems));
  }
  async getById(id: string): Promise<Result<Member | undefined, ApiError>> {
    const member = this.items.find((m) => m.id === id);
    return ok(await simulateLatency(member));
  }
  async add(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> {
    const memberToAdd = { ...newItem, id: 'new-member-id' };
    this._items.push(memberToAdd);
    return ok(await simulateLatency(memberToAdd));
  }
  async update(updatedItem: Member): Promise<Result<Member, ApiError>> {
    const index = this._items.findIndex((m) => m.id === updatedItem.id);
    if (index !== -1) {
      this._items[index] = updatedItem;
      return ok(await simulateLatency(updatedItem));
    }
    return err({ message: 'Member not found', statusCode: 404 });
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    const initialLength = this._items.length;
    this._items = this._items.filter((m) => m.id !== id);
    if (this._items.length === initialLength) {
      return err({ message: 'Member not found', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }
  async searchMembers(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    let filteredItems = this._items;

    // Simplified search for testing
    if (filters.fullName) {
      const lowerCaseFullName = filters.fullName.toLowerCase();
      filteredItems = filteredItems.filter(m => m.fullName?.toLowerCase().includes(lowerCaseFullName));
    }

    const totalItems = filteredItems.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filteredItems.slice(start, end);

    return ok(await simulateLatency({
      items,
      totalItems,
      totalPages,
    }));
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
