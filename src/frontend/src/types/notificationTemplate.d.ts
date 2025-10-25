import type { BaseEntity } from './base.d';
import type { NotificationChannel, NotificationType } from './notification.d';
import type { TemplateFormat } from './template-format.d';

export interface NotificationTemplate extends BaseEntity {
  eventType: NotificationType;
  channel: NotificationChannel;
  subject: string;
  body: string;
  format: TemplateFormat;
  languageCode: string;
  placeholders?: string;
  isActive: boolean;
}

export interface NotificationTemplateFilter {
  eventType?: NotificationType;
  channel?: NotificationChannel;
  isActive?: boolean;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
