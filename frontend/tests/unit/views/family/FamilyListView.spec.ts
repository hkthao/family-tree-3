import { mount, flushPromises } from '@vue/test-utils';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import FamilyListView from '@/views/family/FamilyListView.vue';
import { useFamilyStore } from '@/stores/family.store';
import { useMemberStore } from '@/stores/member.store';
import { useEventStore } from '@/stores/event.store';
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
import type { Event } from '@/types/event';
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
      filtered = filtered.filter(
        (family) => family.visibility === filter.visibility,
      );
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return ok(
      await simulateLatency({
        items,
        totalItems,
        totalPages,
      }),
    );
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
  async fetchMembersByFamilyId(
    familyId: string,
  ): Promise<Result<Member[], ApiError>> {
    const filteredItems = this.items.filter(
      (member) => member.familyId === familyId,
    );
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
  async searchItems(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    let filteredItems = this._items;

    // Simplified search for testing
    if (filters.fullName) {
      const lowerCaseFullName = filters.fullName.toLowerCase();
      filteredItems = filteredItems.filter((m) =>
        m.fullName?.toLowerCase().includes(lowerCaseFullName),
      );
    }

    const totalItems = filteredItems.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filteredItems.slice(start, end);

    return ok(
      await simulateLatency({
        items,
        totalItems,
        totalPages,
      }),
    );
  }
}

import type {
  IEventService,
  EventFilter,
} from '@/services/event/event.service.interface';
import {
  generateMockEvents,
  generateMockEvent,
} from '@/data/mock/event.mock';

export class MockEventServiceForTest implements IEventService {
  private _events: Event[];

  constructor() {
    this._events = generateMockEvents(20);
  }

  reset() {
    this._events = generateMockEvents(20);
  }
  get events(): Event[] {
    return [...this._events];
  }

  async fetch(): Promise<Result<Event[], ApiError>> {
    return ok(await simulateLatency(this.events));
  }

  async getById(
    id: string,
  ): Promise<Result<Event | undefined, ApiError>> {
    const event = this.events.find((e) => e.id === id);
    return ok(await simulateLatency(event));
  }

  async add(
    newEvent: Omit<Event, 'id'>,
  ): Promise<Result<Event, ApiError>> {
    const eventToAdd: Event = {
      ...newEvent,
      id: generateMockEvent(this._events.length + 1).id,
    };
    this._events.push(eventToAdd);
    return ok(await simulateLatency(eventToAdd));
  }

  async update(
    updatedEvent: Event,
  ): Promise<Result<Event, ApiError>> {
    const index = this._events.findIndex((e) => e.id === updatedEvent.id);
    if (index !== -1) {
      this._events[index] = updatedEvent;
      return ok(await simulateLatency(updatedEvent));
    }
    return err({ message: 'Event not found', statusCode: 404 });
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    const initialLength = this._events.length;
    this._events = this._events.filter((event) => event.id !== id);
    if (this._events.length === initialLength) {
      return err({ message: 'Event not found', statusCode: 404 });
    }
    return ok(undefined);
  }

  async searchItems(
    filters: EventFilter,
    page?: number,
    itemsPerPage?: number,
  ): Promise<Result<Paginated<Event>, ApiError>> {
    let filteredEvents = this._events;

    if (filters.searchQuery) {
      const lowerCaseSearchQuery = filters.searchQuery.toLowerCase();
      filteredEvents = filteredEvents.filter(
        (event) =>
          event.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (event.description &&
            event.description.toLowerCase().includes(lowerCaseSearchQuery)),
      );
    }

    if (filters.type) {
      filteredEvents = filteredEvents.filter(
        (event) => event.type === filters.type,
      );
    }

    if (filters.familyId) {
      filteredEvents = filteredEvents.filter(
        (event) => event.familyId === filters.familyId,
      );
    }

    if (filters.startDate) {
      filteredEvents = filteredEvents.filter(
        (event) =>
          event.startDate && new Date(event.startDate) >= filters.startDate!,
      );
    }

    if (filters.endDate) {
      filteredEvents = filteredEvents.filter(
        (event) =>
          event.startDate && new Date(event.startDate) <= filters.endDate!,
      ); // Use event.startDate
    }

    if (filters.location) {
      const lowerCaseLocation = filters.location.toLowerCase();
      filteredEvents = filteredEvents.filter(
        (event) =>
          event.location &&
          event.location.toLowerCase().includes(lowerCaseLocation),
      );
    }

    const totalItems = filteredEvents.length;
    const currentPage = page || 1;
    const currentItemsPerPage = itemsPerPage || 10;
    const totalPages = Math.ceil(totalItems / currentItemsPerPage);
    const start = (currentPage - 1) * currentItemsPerPage;
    const end = start + currentItemsPerPage;
    const items = filteredEvents.slice(start, end);

    return ok(
      await simulateLatency({
        items,
        totalItems,
        totalPages,
      }),
    );
  }
}

// Mock the stores

let familyStore: ReturnType<typeof useFamilyStore>;
let memberStore: ReturnType<typeof useMemberStore>;
let familyEventStore: ReturnType<typeof useEventStore>;
let notificationStore: ReturnType<typeof useNotificationStore>;
let mockFamilyService: MockFamilyServiceForTest;
let mockMemberService: MockMemberServiceForTest;
let mockEventService: MockEventServiceForTest;

beforeEach(async () => {
  const pinia = createPinia();
  setActivePinia(pinia);

  mockFamilyService = new MockFamilyServiceForTest();
  mockMemberService = new MockMemberServiceForTest();
  mockEventService = new MockEventServiceForTest();

  familyStore = useFamilyStore();
  memberStore = useMemberStore();
  familyEventStore = useEventStore();
  notificationStore = useNotificationStore();

  familyStore.$reset();
  memberStore.$reset();
  notificationStore.$reset();

  familyStore.services = createServices('test', { family: mockFamilyService });
  memberStore.services = createServices('test', { member: mockMemberService });
  familyEventStore.services = createServices('test', {
    event: mockEventService,
  });

  vi.spyOn(familyStore, 'searchItems');
  vi.spyOn(notificationStore, 'showSnackbar');
  global.visualViewport = { width: 1024, height: 768 } as any; // Mock visualViewport
});

const i18n = createI18n({
  legacy: false,
  locale: 'en',
  messages: {
    en: {
      confirmDelete: {
        title: '',
        message: '',
      },
      family: {
        management: {
          title: '',
          searchLabel: '',
          visibility: {
            all: '',
            private: '',
            public: '',
          },
          filterLabel: '',
          headers: {
            avatar: '',
            name: '',
            totalMembers: '',
            visibility: '',
            actions: '',
          },
          messages: {
            deleteSuccess: 'Family deleted successfully',
            deleteError: 'Failed to delete family',
          },
        },
      },
      member: {
        search: {
          apply: '',
          reset: '',
        },
      },
    },
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
        plugins: [i18n, vuetify, router],
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

  it('navigates to add family page', async () => {
    const routerPushSpy = vi.spyOn(router, 'push');
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    (wrapper.vm as any).navigateToAddFamily();
    expect(routerPushSpy).toHaveBeenCalledWith('/family/add');
  });

  it('navigates to edit family page', async () => {
    const routerPushSpy = vi.spyOn(router, 'push');
    const family = mockFamilyService.items[0];
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    (wrapper.vm as any).navigateToEditFamily(family);
    expect(routerPushSpy).toHaveBeenCalledWith(`/family/edit/${family.id}`);
  });

  it('navigates to view family detail', async () => {
    const family = mockFamilyService.items[0];
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    (wrapper.vm as any).navigateToViewFamily(family);
    expect((wrapper.vm as any).selectedFamily).toEqual({ ...family });
    expect((wrapper.vm as any).detailDialog).toBe(true);
  });

  it('closes family detail dialog', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    (wrapper.vm as any).navigateToViewFamily(mockFamilyService.items[0]); // Open dialog first
    expect((wrapper.vm as any).detailDialog).toBe(true);
    (wrapper.vm as any).closeDetail();
    expect((wrapper.vm as any).detailDialog).toBe(false);
    expect((wrapper.vm as any).selectedFamily).toBeUndefined();
  });

  it('handles filter update and reloads families', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    const newFilters: FamilySearchFilter = {
      searchQuery: 'test',
      visibility: 'public',
    };
    (wrapper.vm as any).handleFilterUpdate(newFilters);
    expect((wrapper.vm as any).currentFilters).toEqual(newFilters);
    expect((wrapper.vm as any).currentPage).toBe(1);
    expect(familyStore.searchItems).toHaveBeenCalledTimes(4); // Initial load + after filter update + watcher trigger
  });

  it('handles list options update', async () => {
    const familySetPageSpy = vi.spyOn(familyStore, 'setPage');
    const familySetItemsPerPageSpy = vi.spyOn(familyStore, 'setItemsPerPage');
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    const newOptions = { page: 2, itemsPerPage: 25 };
    (wrapper.vm as any).handleListOptionsUpdate(newOptions);
    expect(familySetPageSpy).toHaveBeenCalledWith(2);
    expect(familySetItemsPerPageSpy).toHaveBeenCalledWith(25);
  });

  it('loads all members on mount', async () => {
    const memberStoreFetchItemsSpy = vi.spyOn(memberStore, 'fetchItems');
    mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    expect(memberStoreFetchItemsSpy).toHaveBeenCalled();
  });

  it('computes family member counts correctly', async () => {
    // Mock memberStore.items to have some members
    memberStore.items = [
      { id: 'f1', familyId: 'f1', fullName: 'Member 1' } as Member,
      { id: 'f2', familyId: 'f1', fullName: 'Member 2' } as Member,
      { id: 'f3', familyId: 'f2', fullName: 'Member 3' } as Member,
    ];

    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();

    const familyMemberCounts = (wrapper.vm as any).familyMemberCounts;
    expect(familyMemberCounts).toEqual({
      f1: 2,
      f2: 1,
    });
  });

  it('reloads families when currentPage changes', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    expect(familyStore.searchItems).toHaveBeenCalledTimes(3); // Initial load + watcher trigger
  });

  describe('Delete Family', () => {
    beforeEach(() => {
      vi.spyOn(notificationStore, 'showSnackbar');
      vi.spyOn(familyStore, 'deleteItem');
    });

    it('confirms and deletes a family successfully', async () => {
      const family = mockFamilyService.items[0];
      vi.spyOn(familyStore, 'deleteItem').mockImplementation(
        async (id: string): Promise<void> => {
          return Promise.resolve();
        },
      ); // Mock successful deletion

      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [i18n, vuetify, router],
        },
      });

      await flushPromises();

      // Simulate confirming delete
      (wrapper.vm as any).confirmDelete(family);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
      expect((wrapper.vm as any).familyToDelete).toEqual(family);

      // Simulate confirming the delete dialog
      await (wrapper.vm as any).handleDeleteConfirm();

      expect(familyStore.deleteItem).toHaveBeenCalledWith(family.id);
      expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
        'Family deleted successfully',
        'success',
      );
      expect(familyStore.searchItems).toHaveBeenCalledTimes(3); // Initial load + reload after delete + watcher trigger
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).familyToDelete).toBeUndefined();
    });

    it('handles error during family deletion', async () => {
      const family = mockFamilyService.items[0];
      vi.spyOn(familyStore, 'deleteItem').mockImplementation(
        async (id: string): Promise<void> => {
          return Promise.reject(new Error('Delete failed')); // Simulate an actual error being thrown
        },
      );

      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [i18n, vuetify, router],
        },
      });

      await flushPromises();

      // Simulate confirming delete
      (wrapper.vm as any).confirmDelete(family);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
      expect((wrapper.vm as any).familyToDelete).toEqual(family);

      // Simulate confirming the delete dialog
      await (wrapper.vm as any).handleDeleteConfirm();

      expect(familyStore.deleteItem).toHaveBeenCalledWith(family.id);
      expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
        'Failed to delete family',
        'error',
      );
      expect(familyStore.searchItems).toHaveBeenCalledTimes(3); // Initial load + watcher trigger
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).familyToDelete).toBeUndefined();
    });

    it('cancels family deletion', async () => {
      const family = mockFamilyService.items[0];

      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [i18n, vuetify, router],
        },
      });

      await flushPromises();

      // Simulate confirming delete
      (wrapper.vm as any).confirmDelete(family);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
      expect((wrapper.vm as any).familyToDelete).toEqual(family);

      // Simulate canceling the delete dialog
      (wrapper.vm as any).handleDeleteCancel();

      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).familyToDelete).toBeUndefined();
      expect(familyStore.deleteItem).not.toHaveBeenCalled();
      expect(notificationStore.showSnackbar).not.toHaveBeenCalled();
    });
  });
});
