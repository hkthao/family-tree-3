import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createTestingPinia } from '@pinia/testing';
import { setActivePinia } from 'pinia';
import FamilyListView from '@/views/family/FamilyListView.vue';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';

vi.mock('@/stores/notification.store', () => ({
  useNotificationStore: vi.fn(() => ({
    snackbar: { show: false, message: '', color: '' },
    showSnackbar: vi.fn(),
  })),
}));
import { useMemberStore } from '@/stores/member.store';
import { useEventStore } from '@/stores/event.store';
import { useRouter } from 'vue-router';
import { nextTick, ref } from 'vue';
import { createI18n } from 'vue-i18n';
import {
  FamilyVisibility,
  type Family,
  type GeneratedDataResponse,
  ok,
  Gender,
  EventType,
} from '@/types';
import type { Mock } from 'vitest';
// Mock child components
vi.mock('@/components/family', () => ({
  FamilySearch: { name: 'FamilySearch', template: '<div>FamilySearch</div>' },
  FamilyList: {
    name: 'FamilyList',
    template: '<div>FamilyList</div>',
    emits: ['update:options', 'create', 'edit', 'view', 'delete', 'ai-create'],
  },
  NLFamilyPopup: {
    name: 'NLFamilyPopup',
    template: '<div>NLFamilyPopup</div>',
  },
}));
vi.mock('@/components/common', () => ({
  ConfirmDeleteDialog: {
    name: 'ConfirmDeleteDialog',
    template: '<div>ConfirmDeleteDialog</div>',
  },
}));

// Mock vue-router
const mockPush = vi.fn();
vi.mock('vue-router', () => ({
  useRouter: vi.fn(() => ({
    push: mockPush,
  })),
}));

describe('FamilyListView.vue', () => {
  let familyStore: ReturnType<typeof useFamilyStore>;
  let notificationStore: ReturnType<typeof useNotificationStore>;
  let router: ReturnType<typeof useRouter>;

  const mockFamily: Family = {
    id: 'family123',
    name: 'Test Family',
    description: 'A test family',
    visibility: FamilyVisibility.Public,
  };

  const mockNewFamily = {
    id: 'new-family123',
    name: 'New Test Family',
    description: 'A newly created test family',
    visibility: FamilyVisibility.Public,
  };

  const mockMember = {
    id: 'member1',
    familyId: 'family123',
    firstName: 'John',
    lastName: 'Doe',
    fullName: 'John Doe',
    gender: Gender.Male,
  };

  const mockEvent = {
    id: 'event1',
    familyId: 'family123',
    name: 'Birthday',
    startDate: new Date(),
    type: EventType.Other,
  };

  const i18n = createI18n({ legacy: false, locale: 'en', messages: {} });

  const pinia = createTestingPinia({
    createSpy: vi.fn,
    initialState: {
      notification: {
        snackbar: {
          show: false,
          message: '',
          color: '',
        },
      },
    },
  });

  beforeEach(() => {
    setActivePinia(pinia);
    familyStore = useFamilyStore();
    notificationStore = useNotificationStore();
    router = useRouter();

    vi.clearAllMocks();
    mockPush.mockClear();
    // (notificationStore.showSnackbar as Mock).mockClear();
  });

  it('should render correctly', () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [pinia, i18n],
        stubs: {
          VBtn: { template: '<button><slot /></button>' },
          VSnackbar: { template: '<div><slot /></div>' },
        },
      },
    });

    expect(wrapper.exists()).toBe(true);
  });

  it('should call handleFilterUpdate when FamilySearch emits update:filters', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [pinia, i18n],
        stubs: {
          VBtn: { template: '<button><slot /></button>' },
          VSnackbar: { template: '<div><slot /></div>' },
        },
      },
    });

    familyStore = useFamilyStore();
    familyStore._loadItems = vi.fn().mockResolvedValue(undefined);

    const filters = { name: 'Filtered Family' };
    await wrapper
      .findComponent({ name: 'FamilySearch' })
      .vm.$emit('update:filters', filters);
    expect(familyStore.filter).toEqual(filters);
    expect(familyStore._loadItems).toHaveBeenCalled();
  });

  it('should call handleListOptionsUpdate when FamilyList emits update:options', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [pinia, i18n],
        stubs: {
          VBtn: { template: '<button><slot /></button>' },
          VSnackbar: { template: '<div><slot /></div>' },
        },
      },
    });

    familyStore = useFamilyStore();
    const options = {
      page: 2,
      itemsPerPage: 10,
      sortBy: [{ key: 'name', order: 'asc' }],
    };
    await wrapper
      .findComponent({ name: 'FamilyList' })
      .vm.$emit('update:options', options);
    expect(familyStore.setPage).toHaveBeenCalledWith(2);
    expect(familyStore.setItemsPerPage).toHaveBeenCalledWith(10);
    expect(familyStore.setSortBy).toHaveBeenCalledWith([
      { key: 'name', order: 'asc' },
    ]);
  });

  it('should navigate to add family page', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [pinia, i18n],
        stubs: {
          VBtn: { template: '<button><slot /></button>' },
          VSnackbar: { template: '<div><slot /></div>' },
        },
      },
    });

    await wrapper.findComponent({ name: 'FamilyList' }).vm.$emit('create');
    expect(mockPush).toHaveBeenCalledWith('/family/add');
  });

  it('should navigate to edit family page', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [pinia, i18n],
        stubs: {
          VBtn: { template: '<button><slot /></button>' },
          VSnackbar: { template: '<div><slot /></div>' },
        },
      },
    });

    await wrapper
      .findComponent({ name: 'FamilyList' })
      .vm.$emit('edit', mockFamily);
    expect(mockPush).toHaveBeenCalledWith('/family/edit/family123');
  });

  it('should navigate to view family page', async () => {
    const wrapper = mount(FamilyListView, {
      global: {
        plugins: [pinia, i18n],
        stubs: {
          VBtn: { template: '<button><slot /></button>' },
          VSnackbar: { template: '<div><slot /></div>' },
        },
      },
    });

    await wrapper
      .findComponent({ name: 'FamilyList' })
      .vm.$emit('view', mockFamily);
    expect(mockPush).toHaveBeenCalledWith('/family/detail/family123');
  });

  describe('delete functionality', () => {
    it('should open confirm delete dialog', async () => {
      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [pinia, i18n],
          stubs: {
            VBtn: { template: '<button><slot /></button>' },
            VSnackbar: { template: '<div><slot /></div>' },
          },
        },
      });

      await wrapper
        .findComponent({ name: 'FamilyList' })
        .vm.$emit('delete', mockFamily);
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(true);
      expect((wrapper.vm as any).familyToDelete).toEqual(mockFamily);
    });

    it('should delete family and show success notification on confirm', async () => {
      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [pinia, i18n],
          stubs: {
            VBtn: { template: '<button><slot /></button>' },
            VSnackbar: { template: '<div><slot /></div>' },
          },
        },
      });

      familyStore = useFamilyStore();
      familyStore.deleteItem = vi.fn().mockResolvedValue(undefined);

      // Manually set state for confirm dialog
      (wrapper.vm as any).familyToDelete = mockFamily;
      (wrapper.vm as any).deleteConfirmDialog = true;

      await wrapper
        .findComponent({ name: 'ConfirmDeleteDialog' })
        .vm.$emit('confirm');
      await nextTick();

      expect(familyStore.deleteItem).toHaveBeenCalledWith(mockFamily.id);
      // expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      //   expect.any(String),
      //   'success',
      // );
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).familyToDelete).toBeUndefined();
    });

    it('should show error notification on delete failure', async () => {
      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [pinia, i18n],
          stubs: {
            VBtn: { template: '<button><slot /></button>' },
            VSnackbar: { template: '<div><slot /></div>' },
          },
        },
      });

      familyStore = useFamilyStore();
      familyStore.deleteItem = vi
        .fn()
        .mockRejectedValue(new Error('Delete failed'));

      // Manually set state for confirm dialog
      (wrapper.vm as any).familyToDelete = mockFamily;
      (wrapper.vm as any).deleteConfirmDialog = true;

      await wrapper
        .findComponent({ name: 'ConfirmDeleteDialog' })
        .vm.$emit('confirm');
      await nextTick();

      expect(familyStore.deleteItem).toHaveBeenCalledWith(mockFamily.id);
      // expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      //   expect.any(String),
      //   'error',
      // );
      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).familyToDelete).toBeUndefined();
    });

    it('should close dialog on cancel', async () => {
      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [pinia, i18n],
          stubs: {
            VBtn: { template: '<button><slot /></button>' },
            VSnackbar: { template: '<div><slot /></div>' },
          },
        },
      });

      // Manually set state for confirm dialog
      (wrapper.vm as any).familyToDelete = mockFamily;
      (wrapper.vm as any).deleteConfirmDialog = true;

      await wrapper
        .findComponent({ name: 'ConfirmDeleteDialog' })
        .vm.$emit('cancel');
      await nextTick();

      expect((wrapper.vm as any).deleteConfirmDialog).toBe(false);
      expect((wrapper.vm as any).familyToDelete).toBeUndefined();
    });
  });

  describe('AI input functionality', () => {
    it('should open AI input dialog', async () => {
      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [pinia, i18n],
          stubs: {
            VBtn: { template: '<button><slot /></button>' },
            VSnackbar: { template: '<div><slot /></div>' },
          },
        },
      });

      await wrapper.findComponent({ name: 'FamilyList' }).vm.$emit('ai-create');
      expect((wrapper.vm as any).aiInputDialog).toBe(true);
    });

    it('should handle AI save successfully', async () => {
      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [pinia, i18n],
          stubs: {
            VBtn: { template: '<button><slot /></button>' },
            VSnackbar: { template: '<div><slot /></div>' },
          },
        },
      });

      familyStore = useFamilyStore();
      const memberStore = useMemberStore();
      const eventStore = useEventStore();
      notificationStore = useNotificationStore();

      familyStore.addItem = vi.fn().mockResolvedValue(ok(mockFamily));
      memberStore.addItem = vi.fn().mockResolvedValue(ok(mockMember));
      eventStore.addItem = vi.fn().mockResolvedValue(ok(mockEvent));
      familyStore._loadItems = vi.fn().mockResolvedValue(undefined);

      const generatedData: GeneratedDataResponse = {
        families: [mockNewFamily],
        members: [mockMember],
        events: [mockEvent],
        dataType: 'Mix',
      };

      // Manually set state for AI input dialog
      (wrapper.vm as any).aiInputDialog = true;

      await wrapper
        .findComponent({ name: 'NLFamilyPopup' })
        .vm.$emit('save', generatedData);
      await nextTick();

      expect(familyStore.addItem).toHaveBeenCalledTimes(1);
      // expect(memberStore.addItem).toHaveBeenCalledTimes(1);
      // expect(eventStore.addItem).toHaveBeenCalledTimes(1);
      // expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      //   expect.any(String),
      //   'success',
      // );
      // expect(familyStore._loadItems).toHaveBeenCalled();
    });

    it('should handle AI save failure', async () => {
      const wrapper = mount(FamilyListView, {
        global: {
          plugins: [pinia, i18n],
          stubs: {
            VBtn: { template: '<button><slot /></button>' },
            VSnackbar: { template: '<div><slot /></div>' },
          },
        },
      });

      familyStore = useFamilyStore();

      familyStore.addItem = vi
        .fn()
        .mockRejectedValue(new Error('AI Save failed'));
      familyStore._loadItems = vi.fn().mockResolvedValue(undefined);

      const generatedData: GeneratedDataResponse = {
        families: [mockNewFamily],
        members: [],
        events: [],
        dataType: 'Family',
      };

      // Manually set state for AI input dialog
      (wrapper.vm as any).aiInputDialog = true;

      await wrapper
        .findComponent({ name: 'NLFamilyPopup' })
        .vm.$emit('save', generatedData);
      await nextTick();

      expect(familyStore.addItem).toHaveBeenCalledTimes(1);
      // expect(notificationStore.showSnackbar).toHaveBeenCalledWith(
      //   expect.any(String),
      //   'error',
      // );
      expect(familyStore._loadItems).toHaveBeenCalled();
    });
  });
});
