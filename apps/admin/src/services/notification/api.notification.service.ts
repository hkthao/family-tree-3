import type { ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '../common/api.crud.service';
import type { INotificationService } from './notification.service.interface';
import type { Result } from '@/types'; // Assuming Result type is available globally or via '@/types'

export class ApiNotificationService
  extends ApiCrudService<any, any, any>
  implements INotificationService
{
  constructor(apiClient: ApiClientMethods) {
    // 'notification' is the base path for notification related APIs
    super(apiClient, 'notification');
  }

  async syncSubscriber(): Promise<void> {
    try {
      const result = await this.api.post<Result<void>>(`${this.baseUrl}/subscriber`);
      if (result.ok) {
        console.log('Novu subscriber synced successfully.');
      } else {
        console.error('Failed to sync Novu subscriber:', result.error);
      }
    } catch (error) {
      console.error('Error syncing Novu subscriber:', error);
    }
  }
}
