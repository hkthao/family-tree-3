import { defineStore } from 'pinia';
import type { NotificationTemplate, NotificationTemplateFilter } from '@/types';
import { NotificationType, NotificationChannel, TemplateFormat } from '@/types';
import type { Paginated } from '@/types/pagination.d';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { ApiError } from '@/plugins/axios';
import type { Result } from '@/types';

export const useNotificationTemplateStore = defineStore('notificationTemplate', {
  state: () => ({
    items: [] as NotificationTemplate[],
    currentItem: null as NotificationTemplate | null,
    loading: false,
    error: null as string | null,
    filter: {} as NotificationTemplateFilter,
    totalItems: 0,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    totalPages: 1,
    sortBy: [] as { key: string; order?: 'asc' | 'desc' }[],
  }),
  getters: {},
  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;
      const result = await this.services.notificationTemplate.loadItems(
        {
          ...this.filter,
          sortBy: this.sortBy.length > 0 ? this.sortBy[0].key : undefined,
          sortOrder:
            this.sortBy.length > 0
              ? (this.sortBy[0].order as 'asc' | 'desc' | undefined)
              : undefined,
        },
        this.currentPage,
        this.itemsPerPage,
      );

      if (result.ok && result.value && result.value.items) {
        this.items.splice(0, this.items.length, ...result.value.items);
        this.totalItems = result.value.totalItems;
        this.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('notificationTemplate.errors.load');
        this.items.splice(0, this.items.length);
        this.totalItems = 0;
        this.totalPages = 1;
      }
      this.loading = false;
    },

    async addItem(newItem: Omit<NotificationTemplate, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Result<NotificationTemplate, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.notificationTemplate.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('notificationTemplate.errors.add');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async updateItem(updatedItem: NotificationTemplate): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.notificationTemplate.update(updatedItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('notificationTemplate.errors.update');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.notificationTemplate.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('notificationTemplate.errors.delete');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async generateAiContent(
      prompt: string,
    ): Promise<Result<{ subject: string; body: string }, ApiError>> {
      this.loading = true;
      this.error = null;
      const result = await this.services.notificationTemplate.generateAiContent(
        prompt,
      );
      if (!result.ok) {
        this.error = i18n.global.t('notificationTemplate.errors.aiGenerationFailed');
        console.error(result.error);
      }
      this.loading = false;
      return result;
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages && this.currentPage !== page) {
        this.currentPage = page;
        this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0 && this.itemsPerPage !== count) {
        this.itemsPerPage = count;
        this.currentPage = 1;
        this._loadItems();
      }
    },

    setSortBy(sortBy: { key: string; order?: 'asc' | 'desc' }[]) {
      this.sortBy = sortBy;
      this.currentPage = 1;
      this._loadItems();
    },

    async getById(id: string): Promise<NotificationTemplate | undefined> {
      this.loading = true;
      this.error = null;
      const result = await this.services.notificationTemplate.getById(id);
      this.loading = false;
      if (result.ok) {
        this.currentItem = result.value;
        return result.value;
      } else {
        this.error = i18n.global.t('notificationTemplate.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    setFilter(filter: NotificationTemplateFilter) {
      this.filter = filter;
      this.currentPage = 1;
      this._loadItems();
    },
  },
});
