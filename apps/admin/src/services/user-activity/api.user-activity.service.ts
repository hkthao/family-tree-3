import type { Paginated, RecentActivity, Result, TargetType } from '@/types';
import type { ICurrentUserActivityService } from './user-activity.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

export class ApICurrentUserActivityService implements ICurrentUserActivityService {
  constructor(private http: ApiClientMethods) { }

  async getRecentActivities(
    page: number,
    itemsPerPage?: number,
    targetType?: TargetType,
    targetId?: string,
    groupId?: string,
  ): Promise<Result<Paginated<RecentActivity>, ApiError>> {
    const params = new URLSearchParams();
    if (page) params.append('page', page.toString())
    if (itemsPerPage) params.append('itemsPerPage', itemsPerPage.toString());
    if (targetType !== undefined) params.append('targetType', targetType.toString());
    if (targetId) params.append('targetId', targetId);
    if (groupId) params.append('groupId', groupId);
    return this.http.get<Paginated<RecentActivity>>(`/activity/recent?${params.toString()}`);
  }
}
