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
import { createPinia, setActivePinia } from 'pinia';
import { createServices } from '@/services/service.factory';
import type { IFamilyService } from '@/services/family/family.service.interface';
import type { IMemberService } from '@/services/member/member.service.interface';
import { simulateLatency } from '@/utils/mockUtils';
import type { ApiError } from '@/plugins/axios';
import type { IEventService } from '@/services/event/event.service.interface';
import events from '@/data/mock/events.json';
import families from '@/data/mock/families.json';

import {
  err,
  type Event,
  type Family,
  type Result,
  ok,
  type FamilyFilter,
  type Paginated,
  type Member,
  type MemberFilter,
  FamilyVisibility,
  type EventFilter,
} from '@/types';

// Mock services
class MockFamilyServiceForTest implements IFamilyService {
  public items: Family[] = families as Family[]; // Use a private variable

  async fetch(): Promise<Result<Family[], ApiError>> {
    return ok(await simulateLatency(this.items));
  }
  async getById(id: string): Promise<Result<Family | undefined, ApiError>> {
    return ok(await simulateLatency(this.items.find((f) => f.id === id)));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    const familyToAdd = { ...newItem, id: 'new-family-id' };
    this.items.push(familyToAdd);
    return ok(await simulateLatency(familyToAdd));
  }
  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    const index = this.items.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this.items[index] = updatedItem;
      return ok(await simulateLatency(updatedItem));
    }
    return err({ name: 'ApiError', message: 'Family not found', statusCode: 404 });
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    const initialLength = this.items.length;
    this.items = this.items.filter((f) => f.id !== id);
    if (this.items.length === initialLength) {
      return err({ name: 'ApiError', message: 'Family not found', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }

  async loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    let filtered = this.items;

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

  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    const families = this.items.filter((f) => ids.includes(f.id));
    return ok(await simulateLatency(families));
  }

  async addItems(newItems: Omit<Family, 'id'>[]): Promise<Result<string[], ApiError>> {
    const newIds: string[] = [];
    newItems.forEach(newItem => {
      const newId = (this.items.length + 1).toString();
      const itemToAdd = { ...newItem, id: newId };
      this.items.push(itemToAdd as Family);
      newIds.push(newId);
    });
    return ok(await simulateLatency(newIds));
  }
}

class MockMemberServiceForTest implements IMemberService {
  private items: Member[] = []; // No members initially

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
    this.items.push(memberToAdd);
    return ok(await simulateLatency(memberToAdd));
  }
  async update(updatedItem: Member): Promise<Result<Member, ApiError>> {
    const index = this.items.findIndex((m) => m.id === updatedItem.id);
    if (index !== -1) {
      this.items[index] = updatedItem;
      return ok(await simulateLatency(updatedItem));
    }
    return err({ name: 'ApiError', message: 'Member not found', statusCode: 404 });
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    const initialLength = this.items.length;
    this.items = this.items.filter((m) => m.id !== id);
    if (this.items.length === initialLength) {
      return err({ name: 'ApiError', message: 'Member not found', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }
  async loadItems(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    let filteredItems = this.items;

    // Simplified search for testing
    if (filters.searchQuery) {
      const lowerCaseSearchQuery = filters.searchQuery.toLowerCase();
      filteredItems = filteredItems.filter((m) =>
        m.fullName?.toLowerCase().includes(lowerCaseSearchQuery),
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

  async getByIds(ids: string[]): Promise<Result<Member[], ApiError>> {
    const members = this.items.filter((m) => ids.includes(m.id));
    return ok(await simulateLatency(members));
  }

  async addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>> {
    const newIds: string[] = [];
    newItems.forEach(newItem => {
      const newId = (this.items.length + 1).toString();
      const itemToAdd = { ...newItem, id: newId };
      this.items.push(itemToAdd as Member);
      newIds.push(newId);
    });
    return ok(await simulateLatency(newIds));
  }

  async updateMemberBiography(memberId: string, biographyContent: string): Promise<Result<void, ApiError>> {
    const index = this.items.findIndex((m) => m.id === memberId);
    if (index !== -1) {
      this.items[index].biography = biographyContent; // Assuming 'biography' property exists on Member
      return ok(await simulateLatency(undefined));
    }
    return err({ name: 'ApiError', message: 'Member not found', statusCode: 404 });
  }
}

class MockEventServiceForTest implements IEventService {
  private _events: Event[] = events as unknown as Event[];
  reset() {
    this._events = events as unknown as Event[];
  }

  async fetch(): Promise<Result<Event[], ApiError>> {
    return ok(await simulateLatency(this._events));
  }

  async getById(id: string): Promise<Result<Event | undefined, ApiError>> {
    const event = this._events.find((e) => e.id === id);
    return ok(await simulateLatency(event));
  }

  async add(newEvent: Omit<Event, 'id'>): Promise<Result<Event, ApiError>> {
    const eventToAdd: Event = {
      ...newEvent,
      id: new Date().getTime().toString(),
    };
    this._events.push(eventToAdd);
    return ok(await simulateLatency(eventToAdd));
  }

  async update(updatedEvent: Event): Promise<Result<Event, ApiError>> {
    const index = this._events.findIndex((e) => e.id === updatedEvent.id);
    if (index !== -1) {
      this._events[index] = updatedEvent;
      return ok(await simulateLatency(updatedEvent));
    }
    return err({ name: 'ApiError', message: 'Event not found', statusCode: 404 });
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    const initialLength = this._events.length;
    this._events = this._events.filter((event) => event.id !== id);
    if (this._events.length === initialLength) {
      return err({ name: 'ApiError', message: 'Event not found', statusCode: 404 });
    }
    return ok(undefined);
  }

  async loadItems(
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

  async getByIds(ids: string[]): Promise<Result<Event[], ApiError>> {
    const events = this._events.filter((e) => e.id && ids.includes(e.id));
    return ok(await simulateLatency(events));
  }

  async getUpcomingEvents(familyId?: string): Promise<Result<Event[], ApiError>> {
    let events = this._events;
    if (familyId) {
      events = events.filter((event) => event.familyId === familyId);
    }
    return ok(await simulateLatency(events));
  }

  async addMultiple(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>> {
    const newIds: string[] = [];
    newItems.forEach(newItem => {
      const newId = (this._events.length + 1).toString();
      const itemToAdd = { ...newItem, id: newId };
      this._events.push(itemToAdd as Event);
      newIds.push(newId);
    });
    return ok(await simulateLatency(newIds));
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

  vi.spyOn(familyStore, '_loadItems');
  vi.spyOn(familyStore, 'deleteItem');
  vi.spyOn(notificationStore, 'showSnackbar');
  global.visualViewport = {
    width: 1024,
    height: 768,
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
  } as any; // Mock visualViewport
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
  beforeEach(() => {
    // Reset mocks before each test to ensure clean state
    familyStore.$reset();
    memberStore.$reset();
    notificationStore.$reset();
    vi.clearAllMocks();
  });

  it('renders without errors', () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    expect(wrapper.exists()).toBe(true);
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
    const routerPushSpy = vi.spyOn(router, 'push');
    const family = mockFamilyService.items[0];
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    (wrapper.vm as any).navigateToViewFamily(family);
    expect(routerPushSpy).toHaveBeenCalledWith(`/family/detail/${family.id}`);
  });

  it('handles filter update and reloads families', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    const newFilters: FamilyFilter = {
      searchQuery: 'test',
      visibility: FamilyVisibility.Public,
    };
    (wrapper.vm as any).handleFilterUpdate(newFilters);
    expect((wrapper.vm as any).currentFilters).toEqual(newFilters);
    expect((wrapper.vm as any).currentPage).toBe(1);
    expect(familyStore._loadItems).toHaveBeenCalledTimes(2); // Initial load + after filter update
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

  it('confirms and deletes a family successfully', async () => {
    const family = mockFamilyService.items[0];
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();

    (wrapper.vm as any).confirmDelete(family);
    expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
    expect((wrapper.vm as any).familyToDelete).toEqual(family);

    await (wrapper.vm as any).handleDeleteConfirm();

    expect(familyStore.deleteItem).toHaveBeenCalledWith(family.id);
    expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      'Family deleted successfully',
      'success',
    );
    expect(familyStore._loadItems).toHaveBeenCalled()
    expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
    expect((wrapper.vm as any).familyToDelete).toBeUndefined();
  });

  it('handles error during family deletion', async () => {
    const family = mockFamilyService.items[0];
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();

    (wrapper.vm as any).confirmDelete(family);
    expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
    expect((wrapper.vm as any).familyToDelete).toEqual(family);

    await (wrapper.vm as any).handleDeleteConfirm();

    expect(familyStore.deleteItem).toHaveBeenCalledWith(family.id);
    expect(notificationStore.showSnackbar).toHaveBeenCalled();
    expect(familyStore._loadItems).toHaveBeenCalledTimes(2); // Initial load, no reload on failure
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

    (wrapper.vm as any).confirmDelete(family);
    expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
    expect((wrapper.vm as any).familyToDelete).toEqual(family);

    (wrapper.vm as any).handleDeleteCancel();

    expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
    expect((wrapper.vm as any).familyToDelete).toBeUndefined();
    expect(familyStore.deleteItem).not.toHaveBeenCalled();
    expect(notificationStore.showSnackbar).not.toHaveBeenCalled();
  });

  it('reloads families when currentPage changes', async () => {
    const familyStoreLoadItemsSpy = vi.spyOn(familyStore, '_loadItems');
    mount(FamilyListView, {
      global: {
        plugins: [i18n, vuetify, router],
      },
    });
    await flushPromises();
    familyStoreLoadItemsSpy.mockClear(); // Clear initial load call
    familyStore.setPage(2);
    await flushPromises();
    expect(familyStoreLoadItemsSpy).toHaveBeenCalledTimes(1);
  });
});
