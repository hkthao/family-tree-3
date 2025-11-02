import type { Paginated, RecentActivity, Result, TargetType } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IUserActivityService {
  getRecentActivities(
    page: number,
    pageSize?: number,
    targetType?: TargetType,
    targetId?: string,
    groupId?: string,
  ): Promise<Result<Paginated<RecentActivity>, ApiError>>;
}
