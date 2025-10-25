import type { Result } from '@/types';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError } from '@/plugins/axios';
import type { NotificationTemplate, NotificationTemplateFilter } from '@/types';
import type { Paginated } from '@/types/pagination.d';
import type { INotificationTemplateService } from './notificationTemplate.service.interface';

export class ApiNotificationTemplateService implements INotificationTemplateService {
  constructor(private api: ApiClientMethods) {}

  async loadItems(
    filter?: NotificationTemplateFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: string,
    sortOrder?: 'asc' | 'desc',
  ): Promise<Result<Paginated<NotificationTemplate>, ApiError>> {
    return this.api.get<Paginated<NotificationTemplate>>('/notificationTemplates', {
      params: { ...filter, page, itemsPerPage, sortBy, sortOrder },
    });
  }

  async getById(id: string): Promise<Result<NotificationTemplate, ApiError>> {
    return this.api.get<NotificationTemplate>(`/notificationTemplates/${id}`);
  }

  async add(newItem: Omit<NotificationTemplate, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Result<NotificationTemplate, ApiError>> {
    return this.api.post<NotificationTemplate>('/notificationTemplates', newItem);
  }

  async update(updatedItem: NotificationTemplate): Promise<Result<void, ApiError>> {
    return this.api.put<void>(`/notificationTemplates/${updatedItem.id}`, updatedItem);
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.api.delete<void>(`/notificationTemplates/${id}`);
  }
}
