import type { Result } from '@/types';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError } from '@/plugins/axios';
import type { NotificationTemplate, NotificationTemplateFilter } from '@/types';
import type { Paginated } from '@/types/pagination.d';
import type { INotificationTemplateService } from './notification-template.service.interface';
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiNotificationTemplateService
  implements INotificationTemplateService
{
  constructor(private api: ApiClientMethods) {}
  private baseUrl = `${API_BASE_URL}/notification-template`;

  async loadItems(
    filter?: NotificationTemplateFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: string,
    sortOrder?: 'asc' | 'desc',
  ): Promise<Result<Paginated<NotificationTemplate>, ApiError>> {
    return this.api.get<Paginated<NotificationTemplate>>(`${this.baseUrl}`, {
      params: { ...filter, page, itemsPerPage, sortBy, sortOrder },
    });
  }

  async getById(id: string): Promise<Result<NotificationTemplate, ApiError>> {
    return this.api.get<NotificationTemplate>(`${this.baseUrl}/${id}`);
  }

  async add(
    newItem: Omit<
      NotificationTemplate,
      'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'
    >,
  ): Promise<Result<NotificationTemplate, ApiError>> {
    return this.api.post<NotificationTemplate>(`${this.baseUrl}`, newItem);
  }

  async update(
    updatedItem: NotificationTemplate,
  ): Promise<Result<void, ApiError>> {
    return this.api.put<void>(`${this.baseUrl}/${updatedItem.id}`, updatedItem);
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.api.delete<void>(`${this.baseUrl}/${id}`);
  }

  async generateAiContent(
    prompt: string,
  ): Promise<Result<{ subject: string; body: string }, ApiError>> {
    return this.api.post<{ subject: string; body: string }>(
      `${this.baseUrl}/generate-ai-content`,
      {
        prompt,
      },
    );
  }
}
