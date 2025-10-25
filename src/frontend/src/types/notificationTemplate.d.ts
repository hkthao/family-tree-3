import type { BaseEntity } from './base.d';
import type { NotificationChannel, NotificationType } from './notification.d';

export interface NotificationTemplate extends BaseEntity {
  eventType: NotificationType;
  channel: NotificationChannel;
  subject: string;
  body: string;
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
