import { mount, flushPromises, VueWrapper } from '@vue/test-utils';
import {
  describe,
  it,
  expect,
  vi,
  beforeEach,
  type MockedFunction,
} from 'vitest';
import MemberListView from '@/views/members/MemberListView.vue';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import { createI18n } from 'vue-i18n';
import { createVuetify } from 'vuetify';
import { createRouter, createWebHistory } from 'vue-router';
import type { Member } from '@/types/family';
import type { MemberFilter } from '@/services/member/member.service.interface';
import { createPinia, setActivePinia } from 'pinia';
import { createServices } from '@/services/service.factory';
import type { IMemberService } from '@/services/member/member.service.interface';
import type { Paginated, Result } from '@/types/common';
import { ok, err } from '@/types/common';
import { simulateLatency } from '@/utils/mockUtils';
import type { ApiError } from '@/utils/api';

class MockMemberServiceForTest implements IMemberService {
  private _items: Member[] = [];

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
  async searchMembers(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    let filteredItems = this._items;

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

const mockedShowSnackbar = vi.fn();

vi.mock('@/stores/notification.store', () => ({
  useNotificationStore: vi.fn(() => ({
    snackbar: { show: false, message: '', color: '' },
    showSnackbar: mockedShowSnackbar,
    $reset: vi.fn(),
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

let memberStore: ReturnType<typeof useMemberStore>;
let mockMemberService: MockMemberServiceForTest;

describe('MemberListView.vue', () => {
  let memberStore: ReturnType<typeof useMemberStore>;
  let mockMemberService: MockMemberServiceForTest;
  let notificationStore: ReturnType<typeof useNotificationStore>;

  beforeEach(() => {
    vi.clearAllMocks();
    const pinia = createPinia();
    setActivePinia(pinia);

    mockMemberService = new MockMemberServiceForTest();
    memberStore = useMemberStore();
    memberStore.$reset();
    memberStore.services = createServices('test', {
      member: mockMemberService,
    });

    notificationStore = useNotificationStore();
    notificationStore.$reset();
    mockedShowSnackbar.mockClear();

    vi.spyOn(memberStore, 'setPage');
    vi.spyOn(memberStore, 'setItemsPerPage');
    vi.spyOn(memberStore, 'searchItems');
    vi.spyOn(memberStore, '_loadItems');
  });

  it('renders without errors', () => {
    const wrapper: VueWrapper<InstanceType<typeof MemberListView>> = mount(
      MemberListView,
      { global: { plugins: [i18n, vuetify, router] } },
    );
    expect(wrapper.exists()).toBe(true);
  });

  it('navigates to create view on create button click', async () => {
    const routerPushSpy = vi.spyOn(router, 'push');
    const wrapper = mount(MemberListView, {
      global: { plugins: [i18n, vuetify, router] },
    });
    await (wrapper.vm as any).navigateToCreateView();
    expect(routerPushSpy).toHaveBeenCalledWith('/members/add');
  });

  it('navigates to edit member page on edit button click', async () => {
    const routerPushSpy = vi.spyOn(router, 'push');
    const wrapper = mount(MemberListView, {
      global: { plugins: [i18n, vuetify, router] },
    });
    const member = { id: '1', fullName: 'John Doe' } as Member;
    await (wrapper.vm as any).navigateToEditMember(member);
    expect(routerPushSpy).toHaveBeenCalledWith('/members/edit/1');
  });

  it('opens and closes the view dialog', async () => {
    const wrapper = mount(MemberListView, {
      global: { plugins: [i18n, vuetify, router] },
    });
    const member = { id: '1', fullName: 'John Doe' } as Member;
    await (wrapper.vm as any).openViewDialog(member);
    expect((wrapper.vm as any).viewDialog).toBe(true);
    expect((wrapper.vm as any).selectedMemberForView).toEqual(member);

    await (wrapper.vm as any).closeViewDialog();
    expect((wrapper.vm as any).viewDialog).toBe(false);
    expect((wrapper.vm as any).selectedMemberForView).toBeNull();
  });

  it('handles filter update', async () => {
    const wrapper = mount(MemberListView, {
      global: { plugins: [i18n, vuetify, router] },
    });
    const filters = { fullName: 'John' };
    await (wrapper.vm as any).handleFilterUpdate(filters);
    expect(memberStore.searchItems).toHaveBeenCalledWith(filters);
  });

  it('handles list options update', async () => {
    const wrapper = mount(MemberListView, {
      global: { plugins: [i18n, vuetify, router] },
    });
    const newOptions = { page: 2, itemsPerPage: 25 };
    await (wrapper.vm as any).handleListOptionsUpdate(newOptions);
    expect(memberStore.setPage).toHaveBeenCalledWith(2);
    expect(memberStore.setItemsPerPage).toHaveBeenCalledWith(25);
    expect(memberStore.searchItems).toHaveBeenCalled();
  });

  describe('Delete Member', () => {
    beforeEach(() => {
      vi.spyOn(mockMemberService, 'delete');
    });

    it('opens confirm delete dialog', async () => {
      const member = { id: '1', fullName: 'John Doe' } as Member;

      const wrapper: VueWrapper<InstanceType<typeof MemberListView>> = mount(
        MemberListView,
        {
          global: {
            plugins: [i18n, vuetify, router],
          },
        },
      );

      await flushPromises();

      // Simulate confirming delete
      (wrapper.vm as any).confirmDelete(member);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
      expect((wrapper.vm as any).memberToDelete).toEqual(member);
    });

    it('deletes a member successfully after confirmation', async () => {
      const member = { id: '1', fullName: 'John Doe' } as Member;
      (
        mockMemberService.delete as MockedFunction<
          typeof mockMemberService.delete
        >
      ).mockResolvedValue(ok(undefined));

      const wrapper: VueWrapper<InstanceType<typeof MemberListView>> = mount(
        MemberListView,
        {
          global: {
            plugins: [i18n, vuetify, router],
          },
        },
      );

      await flushPromises();

      // Simulate confirming delete
      (wrapper.vm as any).confirmDelete(member);
      // Simulate confirming the delete dialog
      await (wrapper.vm as any).handleDeleteConfirm();

      expect(mockMemberService.delete).toHaveBeenCalledWith(member.id);
      expect(memberStore.searchItems).toHaveBeenCalled(); // Reload members after deletion
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).memberToDelete).toBeUndefined();
      expect(mockedShowSnackbar).toHaveBeenCalledWith(
        'member.messages.deleteSuccess',
        'success',
      );
    });

    it('handles error during member deletion', async () => {
      const member = { id: '1', fullName: 'John Doe' } as Member;
      const wrapper: VueWrapper<InstanceType<typeof MemberListView>> = mount(
        MemberListView,
        {
          global: {
            plugins: [i18n, vuetify, router],
          },
        },
      );
      (wrapper.vm as any).loadMembers = vi.fn();
      (wrapper.vm as any).loadAllMembers = vi.fn();

      await flushPromises();

      // Simulate confirming delete
      (wrapper.vm as any).confirmDelete(member);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
      expect((wrapper.vm as any).memberToDelete).toEqual(member);

      // Simulate confirming the delete dialog
      await (wrapper.vm as any).handleDeleteConfirm();

      expect(mockMemberService.delete).toHaveBeenCalledWith(member.id);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).memberToDelete).toBeUndefined();
      expect(mockedShowSnackbar).toHaveBeenCalledWith(
        'member.messages.deleteError',
        'error',
      );
    });

    it('cancels member deletion', async () => {
      const member = { id: '1', fullName: 'John Doe' } as Member;

      const wrapper: VueWrapper<InstanceType<typeof MemberListView>> = mount(
        MemberListView,
        {
          global: {
            plugins: [i18n, vuetify, router],
          },
        },
      );

      await flushPromises();

      // Simulate confirming delete
      (wrapper.vm as any).confirmDelete(member);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
      expect((wrapper.vm as any).memberToDelete).toEqual(member);

      // Simulate canceling the delete dialog
      (wrapper.vm as any).handleDeleteCancel();

      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).memberToDelete).toBeUndefined();
      expect(mockMemberService.delete).not.toHaveBeenCalled();
      expect(mockedShowSnackbar).not.toHaveBeenCalled();
    });
  });
});
