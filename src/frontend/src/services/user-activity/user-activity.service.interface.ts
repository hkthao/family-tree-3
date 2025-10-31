import type { PaginatedList, RecentActivity, Result, TargetType } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IUserActivityService {
  getRecentActivities(
    page,
    pageSize?: number,
    targetType?: TargetType,
    targetId?: string,
    groupId?: string,
  ): Promise<Result<PaginatedList<RecentActivity>, ApiError>>;
}
