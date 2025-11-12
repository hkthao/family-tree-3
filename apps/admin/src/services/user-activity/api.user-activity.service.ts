import type { Paginated, RecentActivity, Result, TargetType } from '@/types';
import type { ICurrentUserActivityService } from './user-activity.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApICurrentUserActivityService implements ICurrentUserActivityService {
  private apiUrl = `${API_BASE_URL}/activity`;

  constructor(private http: ApiClientMethods) { }

  async getRecentActivities(
    page: number,
    pageSize?: number,
    targetType?: TargetType,
    targetId?: string,
    groupId?: string,
  ): Promise<Result<Paginated<RecentActivity>, ApiError>> {
    const params = new URLSearchParams();
    if (page) params.append('page', page.toString())
    if (pageSize) params.append('pageSize', pageSize.toString());
    if (targetType !== undefined) params.append('targetType', targetType.toString());
    if (targetId) params.append('targetId', targetId);
    if (groupId) params.append('groupId', groupId);
    return this.http.get<Paginated<RecentActivity>>(`${this.apiUrl}/recent?${params.toString()}`);
  }
}
