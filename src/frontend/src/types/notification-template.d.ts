import type { BaseEntity } from './base.d';
import type { NotificationChannel, NotificationType } from './notification.d';
import type { TemplateFormat } from './template-format.d';
import type { OutputData } from '@editorjs/editorjs';

export interface NotificationTemplate extends BaseEntity {
  eventType: NotificationType;
  channel: NotificationChannel;
  subject: string;
  body: string | OutputData;
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

export interface GenerateNotificationTemplateContentRequest {
  prompt: string;
  notificationType: NotificationType;
  channel: NotificationChannel;
  languageCode: string;
  format: TemplateFormat;
}

export interface GeneratedNotificationTemplateContentResponse {
  subject: string;
  body: string;
}
