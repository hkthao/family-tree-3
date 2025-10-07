import type { IUserActivityService } from './userActivity.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, RecentActivity, TargetType } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiUserActivityService implements IUserActivityService {
  private apiUrl = `${API_BASE_URL}/activities`;

  constructor(private http: ApiClientMethods) {}

  async getRecentActivities(
    limit?: number,
    targetType?: TargetType,
    targetId?: string,
    familyId?: string,
  ): Promise<Result<RecentActivity[], ApiError>> {
    const params = new URLSearchParams();
    if (limit) params.append('limit', limit.toString());
    if (targetType !== undefined) params.append('targetType', targetType.toString());
    if (targetId) params.append('targetId', targetId);
    if (familyId) params.append('familyId', familyId);

    return this.http.get<RecentActivity[]>(`${this.apiUrl}/recent?${params.toString()}`);
  }
}
