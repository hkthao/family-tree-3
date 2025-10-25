import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { NotificationTemplate, NotificationTemplateFilter, NotificationType, NotificationChannel, TemplateFormat } from '@/types';
import type { Paginated } from '@/types/pagination.d';

export interface INotificationTemplateService {
  loadItems(
    filter?: NotificationTemplateFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: string,
    sortOrder?: 'asc' | 'desc',
  ): Promise<Result<Paginated<NotificationTemplate>, ApiError>>;
  getById(id: string): Promise<Result<NotificationTemplate, ApiError>>;
  add(newItem: Omit<NotificationTemplate, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Result<NotificationTemplate, ApiError>>;
  update(updatedItem: NotificationTemplate): Promise<Result<void, ApiError>>;
  delete(id: string): Promise<Result<void, ApiError>>;
  generateAiContent(
    prompt: string,
  ): Promise<Result<{ subject: string; body: string }, ApiError>>;}
