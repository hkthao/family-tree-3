import type { Result } from '@/types';
import { ApiError } from '@/plugins/axios';
import type { NotificationTemplate, NotificationTemplateFilter } from '@/types';
import type { PaginatedItems } from '@/types/pagination.d';
import type { INotificationTemplateService } from './notificationTemplate.service.interface';
import { NotificationChannel, NotificationType } from '@/types';

export class MockNotificationTemplateService implements INotificationTemplateService {
  private mockTemplates: NotificationTemplate[] = [
    {
      id: '1',
      eventType: NotificationType.NewFamilyMember,
      channel: NotificationChannel.InApp,
      subject: 'Thành viên mới trong gia đình {{FamilyName}}',
      body: 'Thành viên {{NewMemberName}} đã được thêm vào gia đình {{FamilyName}} của bạn.',
      isActive: true,
      created: new Date().toISOString(),
      createdBy: 'system',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'system',
    },
    {
      id: '2',
      eventType: NotificationType.FamilyUpdated,
      channel: NotificationChannel.InApp,
      subject: 'Cập nhật thông tin gia đình {{FamilyName}}',
      body: 'Thông tin gia đình {{FamilyName}} của bạn đã được cập nhật.',
      isActive: true,
      created: new Date().toISOString(),
      createdBy: 'system',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'system',
    },
    {
      id: '3',
      eventType: NotificationType.MemberCreated,
      channel: NotificationChannel.InApp,
      subject: 'Thành viên mới {{MemberName}} đã được tạo',
      body: 'Thành viên {{MemberName}} đã được thêm vào gia đình {{FamilyName}}.',
      isActive: true,
      created: new Date().toISOString(),
      createdBy: 'system',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'system',
    },
    {
      id: '4',
      eventType: NotificationType.BirthdayReminder,
      channel: NotificationChannel.Email,
      subject: 'Nhắc nhở sinh nhật {{MemberName}}',
      body: 'Hôm nay là sinh nhật của {{MemberName}}! Đừng quên gửi lời chúc mừng nhé.',
      isActive: true,
      created: new Date().toISOString(),
      createdBy: 'system',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'system',
    },
  ];

  async loadItems(
    filter?: NotificationTemplateFilter,
    page: number = 1,
    itemsPerPage: number = 10,
    sortBy?: string,
    sortOrder?: 'asc' | 'desc',
  ): Promise<Result<PaginatedItems<NotificationTemplate>, ApiError>> {
    let filteredItems = [...this.mockTemplates];

    if (filter) {
      if (filter.eventType !== undefined) {
        filteredItems = filteredItems.filter(item => item.eventType === filter.eventType);
      }
      if (filter.channel !== undefined) {
        filteredItems = filteredItems.filter(item => item.channel === filter.channel);
      }
      if (filter.isActive !== undefined) {
        filteredItems = filteredItems.filter(item => item.isActive === filter.isActive);
      }
      if (filter.search) {
        const searchTerm = filter.search.toLowerCase();
        filteredItems = filteredItems.filter(
          item =>
            item.subject.toLowerCase().includes(searchTerm) ||
            item.body.toLowerCase().includes(searchTerm),
        );
      }
    }

    if (sortBy && sortOrder) {
      filteredItems.sort((a, b) => {
        const aValue = (a as any)[sortBy];
        const bValue = (b as any)[sortBy];

        if (aValue < bValue) return sortOrder === 'asc' ? -1 : 1;
        if (aValue > bValue) return sortOrder === 'asc' ? 1 : -1;
        return 0;
      });
    }

    const totalItems = filteredItems.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const startIndex = (page - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const pagedItems = filteredItems.slice(startIndex, endIndex);

    return Result.ok({ items: pagedItems, totalItems, totalPages });
  }

  async getById(id: string): Promise<Result<NotificationTemplate, ApiError>> {
    const template = this.mockTemplates.find(t => t.id === id);
    if (template) {
      return Result.ok(template);
    } else {
      return Result.fail(new ApiError('Template not found', 404));
    }
  }

  async add(newItem: Omit<NotificationTemplate, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Result<NotificationTemplate, ApiError>> {
    const newTemplate: NotificationTemplate = {
      ...newItem,
      id: (this.mockTemplates.length + 1).toString(),
      created: new Date().toISOString(),
      createdBy: 'mockUser',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'mockUser',
    };
    this.mockTemplates.push(newTemplate);
    return Result.ok(newTemplate);
  }

  async update(updatedItem: NotificationTemplate): Promise<Result<void, ApiError>> {
    const index = this.mockTemplates.findIndex(t => t.id === updatedItem.id);
    if (index !== -1) {
      this.mockTemplates[index] = { ...updatedItem, lastModified: new Date().toISOString(), lastModifiedBy: 'mockUser' };
      return Result.ok(undefined);
    } else {
      return Result.fail(new ApiError('Template not found', 404));
    }
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    const initialLength = this.mockTemplates.length;
    this.mockTemplates = this.mockTemplates.filter(t => t.id !== id);
    if (this.mockTemplates.length < initialLength) {
      return Result.ok(undefined);
    } else {
      return Result.fail(new ApiError('Template not found', 404));
    }
  }
}
