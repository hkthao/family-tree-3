import { setActivePinia, createPinia } from 'pinia';
import { useNotificationTemplateStore } from '@/stores/notification-template.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { NotificationTemplate, Paginated } from '@/types';
import { NotificationType, NotificationChannel, TemplateFormat } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the INotificationTemplateService
const mockLoadItems = vi.fn();
const mockAdd = vi.fn();
const mockUpdate = vi.fn();
const mockDelete = vi.fn();
const mockGetById = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    notificationTemplate: {
      loadItems: mockLoadItems,
      add: mockAdd,
      update: mockUpdate,
      delete: mockDelete,
      getById: mockGetById,
    },
    // Add other services as empty objects if they are not directly used
    ai: {},
    auth: {},
    chat: {},
    chunk: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    family: {},
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      locale: { value: 'en' },
      t: vi.fn((key) => key),
    },
  },
}));

describe('notification-template.store', () => {
  let store: ReturnType<typeof useNotificationTemplateStore>;

  const mockNotificationTemplate: NotificationTemplate = {
    id: 'template-1',

    subject: 'Test Subject',
    body: 'Test Body',

    eventType: NotificationType.FamilyCreated, // Added missing property
    channel: NotificationChannel.Email, // Added missing property
    languageCode: 'vi',
    format: TemplateFormat.PlainText,
    isActive: false,
    created: new Date().toISOString(),
    createdBy: 'user-1',
    lastModified: null,
    lastModifiedBy: null,
  };

  const mockPaginatedNotificationTemplates: Paginated<NotificationTemplate> = {
    items: [mockNotificationTemplate],
    totalItems: 1,
    totalPages: 1,
  };

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);

    // Reset mocks before each test
    mockLoadItems.mockReset();
    mockAdd.mockReset();
    mockUpdate.mockReset();
    mockDelete.mockReset();
    mockGetById.mockReset();

    store = useNotificationTemplateStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('mock');

    // Set default mock resolved values
    mockLoadItems.mockResolvedValue(ok(mockPaginatedNotificationTemplates));
    mockAdd.mockResolvedValue(ok(mockNotificationTemplate));
    mockUpdate.mockResolvedValue(ok(undefined));
    mockDelete.mockResolvedValue(ok(undefined));
    mockGetById.mockResolvedValue(ok(mockNotificationTemplate));
  });

  it('should have correct initial state', () => {
    expect(store.items).toEqual([]);
    expect(store.currentItem).toBeNull();
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
    expect(store.filter).toEqual({});
    expect(store.totalItems).toBe(0);
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(10);
    expect(store.totalPages).toBe(1);
    expect(store.sortBy).toEqual([]);
  });

  describe('_loadItems', () => {
    it('should load items successfully', async () => {
      mockLoadItems.mockResolvedValue(ok(mockPaginatedNotificationTemplates));
      await store._loadItems();

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.items).toEqual([mockNotificationTemplate]);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });

    it('should handle load items failure', async () => {
      const errorMessage = 'Failed to load notification templates.';
      mockLoadItems.mockResolvedValue(
        err({ message: errorMessage } as ApiError),
      );

      await store._loadItems();

      expect(store.loading).toBe(false);
      expect(store.error).toBe('notificationTemplate.errors.load');
      expect(store.items).toEqual([]);
      expect(store.totalItems).toBe(0);
      expect(store.totalPages).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1);
    });
  });

  describe('addItem', () => {
    it('should add an item successfully', async () => {
      const newItem = { ...mockNotificationTemplate, id: undefined };
      const result = await store.addItem(newItem);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockAdd).toHaveBeenCalledWith(newItem);
      expect(mockLoadItems).toHaveBeenCalledTimes(1); // Called once after add
    });

    it('should handle add item failure', async () => {
      const errorMessage = 'Failed to add notification template.';
      mockAdd.mockResolvedValue(err({ message: errorMessage } as ApiError));
      const newItem = { ...mockNotificationTemplate, id: undefined };

      const result = await store.addItem(newItem);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('notificationTemplate.errors.add');
      expect(mockAdd).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(0); // Not called after failed add
    });
  });

  describe('updateItem', () => {
    it('should update an item successfully', async () => {
      const result = await store.updateItem(mockNotificationTemplate);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockUpdate).toHaveBeenCalledWith(mockNotificationTemplate);
      expect(mockLoadItems).toHaveBeenCalledTimes(1); // Called once after update
    });

    it('should handle update item failure', async () => {
      const errorMessage = 'Failed to update notification template.';
      mockUpdate.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.updateItem(mockNotificationTemplate);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('notificationTemplate.errors.update');
      expect(mockUpdate).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(0); // Not called after failed update
    });
  });

  describe('deleteItem', () => {
    it('should delete an item successfully', async () => {
      const result = await store.deleteItem(mockNotificationTemplate.id!);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockDelete).toHaveBeenCalledWith(mockNotificationTemplate.id);
      expect(mockLoadItems).toHaveBeenCalledTimes(1); // Called once after delete
    });

    it('should handle delete item failure', async () => {
      const errorMessage = 'Failed to delete notification template.';
      mockDelete.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.deleteItem(mockNotificationTemplate.id!);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('notificationTemplate.errors.delete');
      expect(mockDelete).toHaveBeenCalledTimes(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(0); // Not called after failed delete
    });
  });

  describe('setPage', () => {
    it('should set the page and reload items if page is valid and different', async () => {
      store.totalPages = 5;
      store.currentPage = 1;
      await store.setPage(2);

      expect(store.currentPage).toBe(2);
      expect(mockLoadItems).toHaveBeenCalledTimes(1); // Called once after setPage
    });

    it('should not set the page or reload items if page is invalid', async () => {
      store.totalPages = 5;
      store.currentPage = 1;
      await store.setPage(0);

      expect(store.currentPage).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(0); // Only called in beforeEach
    });

    it('should not set the page or reload items if page is the same', async () => {
      store.totalPages = 5;
      store.currentPage = 1;
      await store.setPage(1);

      expect(store.currentPage).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(0); // Only called in beforeEach
    });
  });

  describe('setItemsPerPage', () => {
    it('should set items per page and reload items if count is valid and different', async () => {
      store.itemsPerPage = 10;
      await store.setItemsPerPage(20);

      expect(store.itemsPerPage).toBe(20);
      expect(store.currentPage).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1); // Called once after setItemsPerPage
    });

    it('should not set items per page or reload items if count is invalid', async () => {
      store.itemsPerPage = 10;
      await store.setItemsPerPage(0);

      expect(store.itemsPerPage).toBe(10);
      expect(mockLoadItems).toHaveBeenCalledTimes(0); // Only called in beforeEach
    });

    it('should not set items per page or reload items if count is the same', async () => {
      store.itemsPerPage = 10;
      await store.setItemsPerPage(10);

      expect(store.itemsPerPage).toBe(10);
      expect(mockLoadItems).toHaveBeenCalledTimes(0); // Only called in beforeEach
    });
  });

  describe('setSortBy', () => {
    it('should set sort by and reload items', async () => {
      const sortBy = [{ key: 'name', order: 'asc' as 'asc' }];
      store.sortBy = [];
      await store.setSortBy(sortBy);

      expect(store.sortBy).toEqual(sortBy);
      expect(store.currentPage).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1); // Called once after setSortBy
    });
  });

  describe('getById', () => {
    it('should get an item by ID successfully', async () => {
      const result = await store.getById(mockNotificationTemplate.id!);

      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.currentItem).toEqual(mockNotificationTemplate);
      expect(result).toEqual(mockNotificationTemplate);
      expect(mockGetById).toHaveBeenCalledTimes(1);
      expect(mockGetById).toHaveBeenCalledWith(mockNotificationTemplate.id);
    });

    it('should handle get by ID failure', async () => {
      const errorMessage = 'Failed to load notification template.';
      mockGetById.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const result = await store.getById(mockNotificationTemplate.id!);

      expect(store.loading).toBe(false);
      expect(store.error).toBe('notificationTemplate.errors.loadById');
      expect(store.currentItem).toBeNull();
      expect(result).toBeUndefined();
      expect(mockGetById).toHaveBeenCalledTimes(1);
    });
  });

  describe('setFilter', () => {
    it('should set filter and reload items', async () => {
      const filter = { search: 'Test' };
      store.filter = {};
      await store.setFilter(filter);

      expect(store.filter).toEqual(filter);
      expect(store.currentPage).toBe(1);
      expect(mockLoadItems).toHaveBeenCalledTimes(1); // Called once after setFilter
    });
  });
});
